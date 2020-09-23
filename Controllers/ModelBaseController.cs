using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SaraReportAPI.Models;
using SaraReportAPI.Models.Entities;
using System;
using System.Linq;
using System.Text;

namespace SaraReportAPI.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ModelBaseController: ControllerBase {
        protected IConfiguration Configuration { get; set; }
        protected SaraReportDBContext dBContext { get; set; }
        private static Random Random { get; set; }

        public ModelBaseController(IConfiguration config,
                                   SaraReportDBContext dBContext) {
            Configuration = config;
            this.dBContext = dBContext;
            Random = new Random();
        }

        protected ObjectResult BuildResponseObjectResult(SMGenericResponse res, int code = 0) {
            return StatusCode(code != 0 ? code : res.CheckStatus(), res);
        }

        protected JsonResult BuildResponseJsonResult(SMGenericResponse res, int code = 0) {
            return new JsonResult(res) {
                StatusCode = code != 0 ? code : res.CheckStatus()
            };
        }

        protected string TrimAllNonPrintableCharacters(string text, bool trim = true) {
            try {
                string val = System.Text.RegularExpressions.Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
                return trim ? val.Trim() : val;
            } catch (Exception) {
                return string.Empty;
            }
        }

        protected string TrimAllNonPrintableCharactersEncoding(string text, bool trim = true) {
            try {
                string val = Encoding.ASCII.GetString(
                                Encoding.Convert(
                                    Encoding.UTF8,
                                    Encoding.GetEncoding(
                                        Encoding.ASCII.EncodingName,
                                        new EncoderReplacementFallback(string.Empty),
                                        new DecoderExceptionFallback()
                                        ),
                                    Encoding.UTF8.GetBytes(text)
                                )
                            );
                return trim ? val.Trim() : val;
            } catch (Exception) {
                return string.Empty;
            }
        }

        protected T CastToEnum<T>(object ob) {
            T val = (T)Enum.Parse(typeof(T), ob.ToString());
            return val;
        }

        protected string GenerateRandomAlphanumericString(int length) {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Range(1, length)
                                        .Select(_ => chars[Random.Next(chars.Length)])
                                        .ToArray()
                              );
        }
    }
}
