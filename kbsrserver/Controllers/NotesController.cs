using kbsrserver.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Net.Http.Headers;
using System.Web.Http;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Utilities.Encoders;
using Org.BouncyCastle.Security;
using System.Text;
using Org.BouncyCastle.Crypto.Encodings;
using kbsrserver.Helpers;
using kbsrserver.Attributes;
using System.Data.Entity;
using System.Threading.Tasks;

namespace kbsrserver.Controllers
{
    [Authorize]
    [TwoFactorAuthorize]
    public class NotesController : ApiController
    {
        private const int _sessionKeyExpirationInMinutes = 60;
        private readonly static Random _random = new Random();

        [HttpPost]
        public async Task<bool> RefreshPublicKey(RequestModel model)
        {
            var imei = Request.GetImei();

            if (string.IsNullOrEmpty(imei) || string.IsNullOrEmpty(model.PublicKey))
                throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.BadRequest, "Imei and public key can not be null"));

            using (var context = new ApplicationDbContext())
            {
                var user = await context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);
                if (user == null)
                    throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.InternalServerError, "User not found"));
                var key = await context.Keys.FirstOrDefaultAsync(k => k.Imei == imei && k.User.UserName == User.Identity.Name);
                if (key == null)
                {
                    context.Keys.Add(new UserKey
                    {
                        Imei = imei,
                        PublicKey = BouncyCastleHelper.DbProtection(model.PublicKey),
                        User = user
                    });
                }
                else
                {
                    key.PublicKey = BouncyCastleHelper.DbProtection(model.PublicKey);
                    context.Entry(key).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            }
            return true;
        }

        [HttpPost]
        public async Task<SessionKeyResponse> RefreshSessionKey(RequestModel model)
        {
            var imei = Request.GetImei();
            using (var context = new ApplicationDbContext())
            {
                var keys = await context.Keys.Where(k => k.User.UserName == User.Identity.Name).ToListAsync();
                foreach (var currentKey in keys)
                {
                    currentKey.SessionKeyGenerated = null;
                    context.Entry(currentKey).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();

                var key = await context.Keys.FirstOrDefaultAsync(k => k.User.UserName == User.Identity.Name && k.Imei == imei);
                if (key == null || key.PublicKey == null)
                    throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.BadRequest, "Public key by email not found"));                

                var sessionKey = BouncyCastleHelper.GenerateSerpentKey();
                key.SessionKey = BouncyCastleHelper.DbProtection(sessionKey);
                key.SessionKeyGenerated = DateTime.UtcNow;
                context.Entry(key).State = EntityState.Modified;
                await context.SaveChangesAsync();            
                return new SessionKeyResponse { EncryptedSessionKey = BouncyCastleHelper.EncryptSessionKey(sessionKey, BouncyCastleHelper.DbProtection(key.PublicKey, false)) };
            }
        }

        [HttpPost]
        public async Task<NoteResponse> Post(RequestModel model)
        {
            var imei = Request.GetImei();
            if (string.IsNullOrEmpty(imei) || string.IsNullOrEmpty(model.Name))
                throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.BadRequest, "Imei and name can not be null"));

            using (var context = new ApplicationDbContext())
            {
                var key = await context.Keys.FirstOrDefaultAsync(k => k.User.UserName == User.Identity.Name && k.Imei == imei);
                if (key == null || key.PublicKey == null)
                    throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.BadRequest, "Public key by email not found"));

                if (key.SessionKey == null || key.SessionKeyGenerated == null || (DateTime.UtcNow - key.SessionKeyGenerated.Value).TotalMinutes > _sessionKeyExpirationInMinutes)
                    throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.Forbidden, "Session key not generated or has been expired"));

                var text = await context.Notes.FirstOrDefaultAsync(n => n.Name == model.Name);
                if (text == null)
                    throw new HttpResponseException(Request.CreateCustomErrorResponse(HttpStatusCode.NotFound, "Note with such name not found"));

                return new NoteResponse
                {
                    Name = text.Name,
                    EncryptedText = BouncyCastleHelper.EncryptNote(BouncyCastleHelper.DbProtection(text.Text, false), BouncyCastleHelper.DbProtection(key.SessionKey, false))
                };
            }
        }
    }
}
