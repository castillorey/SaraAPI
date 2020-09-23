using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SaraReportAPI.Models;
using SaraReportAPI.Models.Data.Session;
using SaraReportAPI.Models.Entities;
using SaraReportAPI.Models.Entities.Views;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SaraReportAPI.Controllers {
    [ApiController, Route("api/[controller]")]
    public class SessionController: ModelBaseController {
        public SessionController(IConfiguration configuration,
                         SaraReportDBContext dBContext)
    : base(configuration, dBContext) { }

        [HttpPost, AllowAnonymous, Route("Login")]
        public ObjectResult Login([FromBody] SMUserCredentials data) {
            SMGenericResponse response = new SMGenericResponse() {
                Status = "Success",
                Message = "Credentials Correct"
            };
            try {
                SMUserCredentials credentials = data.CleanData();
                MCredentialsCheckResultClass result = CheckCredentials(credentials);
                switch (CastToEnum<HttpStatusCode>(result.Result)) {
                    case HttpStatusCode.BadRequest: {
                        response.Status = "Error";
                        response.Message = result.Message;
                        response.Data = null;
                        return BuildResponseObjectResult(response, (int)HttpStatusCode.BadRequest);
                    }
                    case HttpStatusCode.InternalServerError: {
                        response.Status = "Error";
                        response.Message = result.Message;
                        response.Data = null;
                        return BuildResponseObjectResult(response, (int)HttpStatusCode.InternalServerError);
                    }
                    case HttpStatusCode.Unauthorized: {
                        response.Status = "Error";
                        response.Message = result.Message;
                        response.Data = null;
                        return BuildResponseObjectResult(response, (int)HttpStatusCode.Unauthorized);
                    }
                    case HttpStatusCode.Accepted: {
                        response.Status = "Success";
                        response.Message = "The user has valid access";

                        result = SearchUserInfoLDAP(credentials);
                        if (result.Result == null) {
                            response.Status = "Error";
                            response.Message = result.Message;
                            response.Data = null;
                            return BuildResponseObjectResult(response, (int)HttpStatusCode.InternalServerError);
                        }

                        List<string> roles = (List<string>)result.Result;
                        if (roles.Count <= 0) {
                            response.Status = "Error";
                            response.Message = "The user has no valid access roles to this application";
                            response.Data = null;
                            return BuildResponseObjectResult(response, (int)HttpStatusCode.Unauthorized);
                        }

                        RosterMeta4 rosterMeta4 = dBContext.RosterMeta4.Include(i => i.User).First(f => f.NetworkLogin == credentials.Username);
                        if (!rosterMeta4.Status) {
                            response.Status = "Error";
                            response.Message = "The user has no access permission due inactivity";
                            response.Data = null;
                            return BuildResponseObjectResult(response, (int)HttpStatusCode.Forbidden);
                        }

                        MUserTokenClaimsClass userTokenClaims = null;
                        try {
                            if (rosterMeta4.User == null) {
                                rosterMeta4.User = new Users() {
                                    EmployeeNumber = rosterMeta4.EmployeeNumber,
                                    DateFirstLogin = DateTime.Now,
                                    DateLastLogin = DateTime.Now
                                };
                            } else {
                                rosterMeta4.User.DateLastLogin = DateTime.Now;
                            }
                            dBContext.SaveChanges();


                            roles.AddRange(GetExtraRoles(rosterMeta4));
                            userTokenClaims = new MUserTokenClaimsClass() {
                                FullName = rosterMeta4.FullName,
                                NetworkLogin = rosterMeta4.NetworkLogin,
                                UserID = rosterMeta4.User.ID,
                                EmployeeNumber = rosterMeta4.EmployeeNumber,
                                EmployeeNumberSup = rosterMeta4.EmployeeNumberSup,
                                ClientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                                Roles = roles.Distinct().ToList(),
                                TokenV = 1.0
                            };



                        } catch (Exception) {
                            response.Status = "Error";
                            response.Message = "Error fetching App/User extra info from requests";
                            response.Data = null;
                            return BuildResponseObjectResult(response, (int)HttpStatusCode.InternalServerError);
                        }


                        response.Data = GetJWTToken(userTokenClaims);
                    }
                    break;
                    default: {
                        response.Status = "Error";
                        response.Message = "Internal server error authenticating with Active Directory (Unknow)";
                        response.Data = null;
                        return BuildResponseObjectResult(response, (int)HttpStatusCode.InternalServerError);
                    }
                }
            } catch (Exception e) {
                response.SetErrorInfo(e);
            }
            return BuildResponseObjectResult(response);
        }

        [HttpPost, AllowAnonymous, Route("DevLogin")]
        public ObjectResult DevLogin([FromBody] SMUserCredentials data) {
            SMGenericResponse response = new SMGenericResponse() {
                Status = "Success",
                Message = "Credentials Correct"
            };
            try {
                SMUserCredentials credentials = data.CleanData();
                MCredentialsCheckResultClass result = SearchUserInfoLDAP(credentials);
                if (result.Result == null) {
                    response.Status = "Error";
                    response.Message = result.Message;
                    response.Data = null;
                    return BuildResponseObjectResult(response, (int)HttpStatusCode.InternalServerError);
                }

                List<string> roles = (List<string>)result.Result;
                if (roles.Count <= 0) {
                    response.Status = "Error";
                    response.Message = "The user has no valid access roles to this application";
                    response.Data = null;
                    return BuildResponseObjectResult(response, (int)HttpStatusCode.Unauthorized);
                }

                Models.Entities.Views.RosterMeta4 rosterMeta4 = dBContext.RosterMeta4.Include(i => i.User).First(f => f.NetworkLogin == credentials.Username);
                if (!rosterMeta4.Status) {
                    response.Status = "Error";
                    response.Message = "The user has no access permission due inactivity";
                    response.Data = null;
                    return BuildResponseObjectResult(response, (int)HttpStatusCode.Forbidden);
                }

                MUserTokenClaimsClass userTokenClaims = null;
                try {
                    if (rosterMeta4.User == null) {
                        rosterMeta4.User = new Users() {
                            EmployeeNumber = rosterMeta4.EmployeeNumber,
                            DateFirstLogin = DateTime.Now,
                            DateLastLogin = DateTime.Now
                        };
                    } else {
                        rosterMeta4.User.DateLastLogin = DateTime.Now;
                    }
                    dBContext.SaveChanges();
                    roles.AddRange(GetExtraRoles(rosterMeta4));
                    userTokenClaims = new MUserTokenClaimsClass() {
                        FullName = rosterMeta4.FullName,
                        NetworkLogin = rosterMeta4.NetworkLogin,
                        UserID = rosterMeta4.User.ID,
                        EmployeeNumber = rosterMeta4.EmployeeNumber,
                        EmployeeNumberSup = rosterMeta4.EmployeeNumberSup,
                        ClientIP = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                        Roles = roles.Distinct().ToList(),
                        TokenV = 1.0
                    };

                } catch (Exception ee) {
                    response.Status = "Error";
                    response.Message = "Error fetching App/User extra info from requests";
                    response.Data = ee.ToString();
                    return BuildResponseObjectResult(response, (int)HttpStatusCode.InternalServerError);
                }
                response.Data = GetJWTToken(userTokenClaims);
            } catch (Exception e) {
                response.SetErrorInfo(e);
            }
            return BuildResponseObjectResult(response);
        }

        private MCredentialsCheckResultClass CheckCredentials(SMUserCredentials credentials) {
            if (credentials == null) {
                return new MCredentialsCheckResultClass() {
                    Message = "Insufficient information for login",
                    Result = HttpStatusCode.BadRequest
                };
            }
            LdapDirectoryIdentifier ldapDirectoryIdentifier = new LdapDirectoryIdentifier(GlobalData.Current.ActiveDirectoryInfo["Server"], false, false);
            NetworkCredential networkCredential = new NetworkCredential(credentials.Username, credentials.Password, credentials.Domain);
            MCredentialsCheckResultClass result = new MCredentialsCheckResultClass() {
                Message = "",
                Result = HttpStatusCode.Unauthorized
            };
            using (LdapConnection ldapConnection = new LdapConnection(ldapDirectoryIdentifier, networkCredential)) {
                try {
                    ldapConnection.Bind();
                    result.Result = HttpStatusCode.Accepted;
                } catch (LdapException ex) {
                    switch (ex.ErrorCode) {
                        case 49: {
                            result.Result = HttpStatusCode.Unauthorized;
                            if (ex.ServerErrorMessage == null) {
                                result.Message = "The supplied credential is invalid";
                            } else {
                                // Getting error codes from http://www-01.ibm.com/support/docview.wss?uid=swg21290631
                                System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"(?<=data\s)\d+[^,]");
                                string code = regex.Match(ex.ServerErrorMessage).Value;
                                switch (code) {
                                    case "525": result.Message = "User not found (AD)"; break;
                                    case "52e": result.Message = "Invalid username/password"; break;
                                    case "530": result.Message = "Not permitted to logon at this time (AD)"; break;
                                    case "531": result.Message = "Not permitted to logon at this workstation (AD)"; break;
                                    case "532": result.Message = "Password expired (AD)"; break;
                                    case "533": result.Message = "Account disabled (AD)"; break;
                                    case "534": result.Message = "The user has not been granted the requested logon type at this machine (AD)"; break;
                                    case "701": result.Message = "Account expired (AD)"; break;
                                    case "773": result.Message = "User must reset password (AD)"; break;
                                    case "775": result.Message = "User account locked, contact your local IT to unlock the account (AD)"; break;
                                    default: result.Result = HttpStatusCode.InternalServerError; break;
                                }
                            }
                        }
                        break;
                        case 81: {
                            result.Result = HttpStatusCode.InternalServerError;
                            result.Message = "Authentication server is down or unavailable (AD)";
                        }
                        break;
                        default: {
                            result.Result = HttpStatusCode.InternalServerError;
                            result.Message = "Internal server error (LDAP)";
                        }
                        break;
                    }
                } catch (Exception) {
                    result.Result = HttpStatusCode.InternalServerError;
                    result.Message = "Cannot establish connection with Active Directory server";
                }
            }
            return result;
        }

        private MCredentialsCheckResultClass SearchUserInfoLDAP(SMUserCredentials credentials) {
            MCredentialsCheckResultClass result = new MCredentialsCheckResultClass() {
                Message = "",
                Result = null
            };
            try {
                string path = "LDAP://" + string.Join(',', new string[] { credentials.Domain.ToLower(), "SYKES", "COM" }.Select(s => "DC=" + s));
                DirectoryEntry directoryEntry = new DirectoryEntry() {
                    Path = path,
                    Username = GlobalData.Current.ActiveDirectoryInfo["AppUserN"],
                    Password = GlobalData.Current.ActiveDirectoryInfo["AppUserP"],
                    AuthenticationType = AuthenticationTypes.Secure
                };
                DirectorySearcher searcher = new DirectorySearcher() {
                    Filter = string.Format("(sAMAccountName={0})", credentials.Username),
                    SearchRoot = directoryEntry
                };
                SearchResult searchResult = searcher.FindOne();
                List<string> roles = new List<string>();
                Dictionary<string, List<string>> secGroupsMaping = dBContext.UserGroupsOrNames
                                                                       .Where(w => w.UserName == null)
                                                                       .Include(i => i.UserPermissions).ThenInclude(ti => ti.Role)
                                                                       .ToDictionary(k => k.UserGroup,
                                                                                     v => v.UserPermissions.Select(x => x.Role.Name).ToList()
                                                                        );
                foreach (string prop in searchResult.Properties["memberof"]) {
                    foreach (string str in prop.Split(',')
                                                .Select(x => x.Trim())
                                                .Where(x => x.Length > 3 && x.StartsWith("CN=", StringComparison.InvariantCultureIgnoreCase))) {
                        string group = str.Substring(3).Trim();
                        foreach (KeyValuePair<string, List<string>> secGroup in secGroupsMaping) {
                            if (secGroup.Key == group) {
                                roles = roles.Concat(secGroup.Value).ToList();
                            }
                        }
                    }
                }
                roles = roles.Distinct().ToList();
                result.Result = roles;
            } catch (Exception e) {
                result.Message = GlobalData.Current.EnableVervose ? e.ToString() : "Error fetching info from Active Directory";
                result.Result = null;
            }
            return result;
        }

        private List<string> GetExtraRoles(Models.Entities.Views.RosterMeta4 user) {
            List<string> result = new List<string>();
            Dictionary<string, List<string>> secNamesMaping = dBContext.UserGroupsOrNames
                                                                       .Where(w => w.UserGroup == null)
                                                                       .Include(i => i.UserPermissions).ThenInclude(ti => ti.Role)
                                                                       .ToDictionary(k => k.UserName.ToLower(),
                                                                                     v => v.UserPermissions.Select(x => x.Role.Name).ToList());
            foreach (KeyValuePair<string, List<string>> secName in secNamesMaping) {
                if (secName.Key == user.NetworkLogin) {
                    result = result.Concat(secName.Value).ToList();
                }
            }
            return result.Distinct().ToList();
        }

        private string GetJWTToken(MUserTokenClaimsClass userTokenClaims) {
            SigningCredentials credentials;
            {
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(GlobalData.Current.Jwt["Key"]); // SecureStringWrapper(KeysJWT[userInfo.AppID]))
                credentials = new SigningCredentials(new SymmetricSecurityKey(bytes), SecurityAlgorithms.HmacSha256Signature);
            }
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            List<Claim> listClaims = userTokenClaims.GetType()
                                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                                    .Where(x => x.Name != "Roles")
                                    .Select(x => {
                                        string typeOfData = ClaimValueTypes.String;
                                        switch (x.PropertyType.Name) {
                                            case "String": typeOfData = ClaimValueTypes.String; break;
                                            case "Int32": typeOfData = ClaimValueTypes.Integer32; break;
                                            case "Boolean": typeOfData = ClaimValueTypes.Boolean; break;
                                            default: typeOfData = ClaimValueTypes.String; break;
                                        }
                                        return new Claim(x.Name, Convert.ToString(x.GetValue(userTokenClaims)), typeOfData);
                                    }).ToList();
            foreach (string x in userTokenClaims.Roles)
                listClaims.Add(new Claim("Roles", x, ClaimValueTypes.String));
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(listClaims),
                Issuer = GlobalData.Current.Jwt["Issuer"],
                Expires = DateTime.UtcNow.AddDays(1),
                Audience = GlobalData.Current.Jwt["Audience"],
                SigningCredentials = credentials
            };

            SecurityToken sectoken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(sectoken);
        }
    }
}
