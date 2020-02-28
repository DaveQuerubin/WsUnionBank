using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;

namespace WSUnionbank
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]


    public class Service1 : System.Web.Services.WebService
    {
        //string sessionId = string.Empty;
        string applicationName = "WsUnionBank";
        //[WebMethod]
        private string Auth(string scope, string sessionId)
        {
            //string sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
            UnionbankAuthenticate dto = new UnionbankAuthenticate();
            string tokenResponse = string.Empty;
            try
            {
                Tools.TLS();
                string clientID = ConfigurationManager.AppSettings["UB_client_id"].ToString();
                string username = ConfigurationManager.AppSettings["UB_username"].ToString();
                string password = ConfigurationManager.AppSettings["UB_password"].ToString();
                string TokenUrl = ConfigurationManager.AppSettings["UB_TokenUrl"].ToString();
                string body = "grant_type=password&client_id=@client_id&username=@username&password=@password&scope=@scope";
                body = body.Replace("@client_id", clientID);
                body = body.Replace("@username", username);
                body = body.Replace("@password", password);
                body = body.Replace("@scope", scope);
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate Request: " + body.ToString(), false);
                var client = new RestClient(ConfigurationManager.AppSettings["UB_TokenUrl"].ToString());
                var request = new RestRequest(Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                request.AddParameter("application/x-www-form-urlencoded", body, ParameterType.RequestBody);
                IRestResponse response = client.Execute(request);
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate Response: " + response.Content, false);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dto = JsonConvert.DeserializeObject<UnionbankAuthenticate>(response.Content);
                    return tokenResponse = "00|" + dto.access_token;
                }
                else
                {
                    UnionbankError errordto = new UnionbankError();
                    //Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " CheckPayment Error Response: " + response.Content, false);
                    errordto = JsonConvert.DeserializeObject<UnionbankError>(response.Content);
                    string code = (errordto.httpCode == null) ? errordto.errors[0].code : errordto.httpCode;
                    string description = (errordto.moreInformation == null) ? (errordto.errors[0].message == null) ? errordto.errors[0].description : errordto.errors[0].message : errordto.moreInformation;
                 
                    return code + "|" + description;
                }


            }
            catch (WebException webEx)
            {
                using (WebResponse response = webEx.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        UnionbankError errordto = new UnionbankError();
                        StreamReader sr = new StreamReader(data);
                        string res = sr.ReadToEnd();
                        Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate WebException Error Response: " + res.ToString(), false);
                        errordto = JsonConvert.DeserializeObject<UnionbankError>(res);
                        string code = (errordto.httpCode == null) ? errordto.errors[0].code : errordto.httpCode;
                        string description = (errordto.moreInformation == null) ? (errordto.errors[0].message == null) ? errordto.errors[0].description : errordto.errors[0].message : errordto.moreInformation;
                        return code + "|"+ description;
                    }
                }
                //tokenResponse = "95|" + webEx.Message.ToString();
                //Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate WebException Error Response: " + webEx.Message.ToString(), false);
            }
            catch (Exception ex)
            {
                tokenResponse = "95|Transaction Failed. Please contact ECPAY.";
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate Exception Error Response: " + ex.Message.ToString(), false);
            }
            return tokenResponse;
        }
        [WebMethod(Description = "API - Validate Account number")]
        public string Validate(string accountNumber, string sessionId)
        {
            //sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
            FundtransferDTO dto = new FundtransferDTO();
            string result = string.Empty;
            try
            {
                Tools.TLS();

                string clientID = ConfigurationManager.AppSettings["UB_client_id"].ToString();
                string secretKey = ConfigurationManager.AppSettings["UB_secret_key"].ToString();
                string partnerID = ConfigurationManager.AppSettings["UB_partner_id"].ToString();
                string ValidateUrl = ConfigurationManager.AppSettings["UB_ValidateUrl"].ToString();
                //get access token in Auth method
                string accessTokenResponse = this.Auth("account_verification", sessionId);
                string[] responseArray = accessTokenResponse.Split('|');
                string responseCode = responseArray[0].ToString();
                string responseMessage = responseArray[1].ToString();
                if (responseCode != "00")
                {
                    return responseCode + "|" + responseMessage;
                }
                //create header
                var client = new RestClient(ValidateUrl+accountNumber);
                var request = new RestRequest(Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("authorization", "Bearer " + responseMessage);
                request.AddHeader("x-ibm-client-id", clientID);
                request.AddHeader("x-ibm-client-secret", secretKey);
                request.AddHeader("x-partner-id", partnerID);
                request.AddHeader("content-type", "application/json");
                //create request url
                string body = ValidateUrl + accountNumber;
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Validate Request: " + body, false);
                IRestResponse response = client.Execute(request);
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Validate Response: " + response.Content, false);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    FundtransferDTO resp = new FundtransferDTO();
                    resp = JsonConvert.DeserializeObject<FundtransferDTO>(response.Content);
                    if (resp.code != "001")
                    {
                        result = resp.code + "|Otherwise";
                        return result;
                    }
                    result = resp.code + "|Active";
                    return result;
                }
                else
                {
                    UnionbankError errordto = new UnionbankError();
                    errordto = JsonConvert.DeserializeObject<UnionbankError>(response.Content);
                    string code = string.Empty;
                    code = (errordto.httpCode != null) ? errordto.httpCode : "";
                    code = (code == "") ? ((errordto.error != null) ? ((errordto.error[0].code != null) ? errordto.error[0].code : "") : "") : code;
                    code = (code == "") ? ((errordto.errors != null) ? ((errordto.errors[0].code != null) ? errordto.errors[0].code : "") : "") : code;
                    string description = string.Empty;
                    description = (errordto.moreInformation != null) ? errordto.moreInformation : "";
                    description = (description == "") ? ((errordto.error != null) ? ((errordto.error[0].message != null) ? errordto.error[0].message : "") : "") : description;
                    description = (description == "") ? ((errordto.errors != null) ? ((errordto.errors[0].message != null) ? errordto.errors[0].message : "") : "") : description;
                    description = (description == "") ? errordto.errors[0].description : description; dto.code = code;
                    dto.description = description;
                    return code + "|" + description;
                }
            }
            catch (Exception ex)
            {
                result = "95|Transaction Failed. Please contact ECPAY.";
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Validate Exception Error Response: " + ex.ToString(), false);
            }
            return result;

        }


        [WebMethod(Description = "API - CashIn")]
        public FundtransferDTO FundTransfer(string accountNumber, string amount, string identifier, string sessionId) 
        {
            //sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
            FundtransferDTO dto = new FundtransferDTO();
            string result = string.Empty;
            try
            {
                Tools.TLS();

                string clientID = ConfigurationManager.AppSettings["UB_client_id"].ToString();
                string secretKey = ConfigurationManager.AppSettings["UB_secret_key"].ToString();
                string partnerID = ConfigurationManager.AppSettings["UB_partner_id"].ToString();
                string fundtransferUrl = ConfigurationManager.AppSettings["UB_FundtransferUrl"].ToString();
                string scope = ConfigurationManager.AppSettings["UB_CashInScope"].ToString();
                //validate identifier and account number

                if (accountNumber.Length > 12)
                {
                    Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " FundTransfer Response: 10|Invalid Account Number", false);
                    dto.code = "10";
                    dto.description = "Failed to Credit Beneficiary Account";
                    return dto;
                }
                if (!Regex.IsMatch(accountNumber, @"^\d+$"))
                {
                    Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " FundTransfer Response: 10|Invalid Account Number", false);
                    dto.code = "10";
                    dto.description = "Failed to Credit Beneficiary Account";
                    return dto;
                }
                //get access token in Auth method
                string accessTokenResponse = this.Auth(scope, sessionId);
                string[] responseArray = accessTokenResponse.Split('|');
                string responseCode = responseArray[0].ToString();
                string responseMessage = responseArray[1].ToString();
                if (responseCode != "00")
                {
                    dto.code = responseCode;
                    dto.description = responseMessage;
                    return dto;
                }
                //create header
                var client = new RestClient(fundtransferUrl);
                var request = new RestRequest(Method.POST);
                request.AddHeader("accept", "application/json");
                request.AddHeader("authorization", "Bearer " + responseMessage);
                request.AddHeader("x-ibm-client-id", clientID);
                request.AddHeader("x-ibm-client-secret", secretKey);
                request.AddHeader("x-partner-id", partnerID);
                request.AddHeader("content-type", "application/json");
                //create body
                FundTransferBody body = new FundTransferBody();
                body.senderRefId = sessionId;
                //body.senderRefId = sessionId.Substring(0, 13);
                body.tranRequestDate = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ss.fff");
                body.accountNo = accountNumber;
                body.amount = new AmountBody();
                body.amount.currency = "PHP";
                body.amount.value = amount;
                body.remarks = "Transfer to " + accountNumber;
                body.particulars = "FundTransfer Particular";
                body.info = new List<InfoBody>();
                body.info.Add(new InfoBody { index = 1, name = "Recipient", value = identifier });
                body.info.Add(new InfoBody { index = 2, name = "Message body", value = "Fund transfer" });
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Fundtransfer Request: " + JsonConvert.SerializeObject(body), false);
                request.AddParameter("application/application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);//post payment
                IRestResponse response = client.Execute(request);
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Fundtransfer Response: " + response.Content, false);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dto = JsonConvert.DeserializeObject<FundtransferDTO>(response.Content);
                    return dto;
                }
                else
                {
                    UnionbankError errordto = new UnionbankError();
                    errordto = JsonConvert.DeserializeObject<UnionbankError>(response.Content);
                    string code = string.Empty;
                    code = (errordto.httpCode != null) ? errordto.httpCode : "";
                    code = (code == "") ? ((errordto.error != null) ? ((errordto.error[0].code != null) ? errordto.error[0].code : "") : "") : code;
                    code = (code == "") ? ((errordto.errors != null) ? ((errordto.errors[0].code != null) ? errordto.errors[0].code : "") : "") : code;
                    string description = string.Empty;
                    description = (errordto.moreInformation != null) ? errordto.moreInformation : "";
                    description = (description == "") ? ((errordto.error != null) ? ((errordto.error[0].message != null) ? errordto.error[0].message : "") : "") : description;
                    description = (description == "") ? ((errordto.errors != null) ? ((errordto.errors[0].message != null) ? errordto.errors[0].message : "") : "") : description;
                    description = (description == "") ? errordto.errors[0].description : description; dto.code = code;
                    //description = (description == "") ? errordto.errors[0].details.message : description; dto.code = code;
                    dto.description = description;
                    return dto;
                }
            }
            catch (Exception ex)
            {
                dto.code = "95";
                dto.description = "Transaction Failed. Please contact ECPAY.";
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Fundtransfer Exception Error Response: " + ex.ToString(), false);
            }
            return dto;

        }


        [WebMethod(Description = "ECPay Process")]
        public TransactDTO Transact(int accountId
                               , int branchId
                               , int userId
                               , string username
                               , string password
                               , string accountNumber
                               , decimal amount
                               , string Identifier)
        {
            string response = string.Empty;
            TransactDTO transactResponse = new TransactDTO();
            string sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
            sessionId = sessionId.Substring(0, 12);
            try
            {
                //this.sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Initial log Transact request -> AccountID:" + accountId.ToString() + 
                    " - BranchID:" + branchId.ToString() + 
                    " - UserID:" + userId.ToString() +
                    " - Username:" + username + 
                    " - AccountNumber:" + accountNumber +
                    " - Amount:" + amount.ToString() +
                    " - Identifier:" + Identifier, false);
                if (amount < Convert.ToDecimal(ConfigurationManager.AppSettings["minAmount"]) || amount > Convert.ToDecimal(ConfigurationManager.AppSettings["maxAmount"]))
                {
                    Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Transact Response: 10|Invalid Amount", false);
                    transactResponse.statusid = "10";
                    transactResponse.description = "Allowed Transaction Amount is P" + Convert.ToDecimal(ConfigurationManager.AppSettings["minAmount"]).ToString() + " to P" + Convert.ToDecimal(ConfigurationManager.AppSettings["maxAmount"]).ToString();
                    return transactResponse;
                }
                if (accountNumber.Length > 12)
                {
                     Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Transact Response: 10|Invalid Account Number", false);
                    transactResponse.statusid = "10";
                    transactResponse.description = "Failed to Credit Beneficiary Account";
                    return transactResponse;
                }
                //int num;
                //if (!Regex.IsMatch(Identifier, @"^\d+$"))
                //{
                //    Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Transact Response: 10|Invalid Account Number", false);
                //    transactResponse.statusid = "10";
                //    transactResponse.description = "Invalid Mobile Number";
                //    return transactResponse;
                //}
                //initiate request
                ECashDTO DBRequest = new ECashDTO();
                DBRequest.AccountId = accountId;
                DBRequest.BranchId = branchId;
                DBRequest.UserId = userId;
                DBRequest.Username = username;
                DBRequest.Password = password;
                DBRequest.AccountNumber = accountNumber;
                DBRequest.Amount = amount;
                DBRequest.MobileNumber = Identifier;
                DBRequest.ServiceName = "UNIONBANK";
                DBRequest.LogID = sessionId;
                //INSERT INITIAL RECORD
                ECashDTO DBResponse = new UnionBankDAO().InsertECashTransaction(DBRequest, applicationName);
                if (!DBResponse.IsSuccessful)//if database insert is failed.
                {
                    transactResponse.statusid = DBResponse.StatusId.ToString();
                    transactResponse.description = DBResponse.Description;
                    return transactResponse;
                }
                //PROCESS API
                FundtransferDTO apiResponse = this.FundTransfer(accountNumber, amount.ToString(), Identifier, sessionId);
                
                //mock data
                //FundtransferDTO apiResponse = new FundtransferDTO();
                //apiResponse.payload = new Payload();
                //apiResponse.payload.code = "TS";
                //apiResponse.payload.description = "Successful transaction";
                //apiResponse.payload.ubpTranId = "UB6391875781961";
                //apiResponse.payload.code = "TF";
                //apiResponse.payload.description = "Transaction has failed";

                DBRequest.StatusId = (apiResponse.code == "TS") ? 0 : Convert.ToInt16(95);
                DBRequest.IsSuccessful = (apiResponse.code == "TS") ? true : false;
                DBRequest.Description = apiResponse.description;
                DBRequest.ServiceTransactionId = apiResponse.ubpTranId;
                //tobe updated
                DBRequest.ReferenceNumber = DBResponse.ReferenceNumber;
                DBRequest.ServiceCharge = DBResponse.ServiceCharge;
                DBRequest.ECashId = DBResponse.ECashId;
                DBRequest.FaceCost = DBResponse.FaceCost;
                //update ecash trans.
                new UnionBankDAO().UpdateECashTrans(DBRequest, applicationName);
                //return response
                transactResponse.statusid = DBRequest.StatusId.ToString();
                transactResponse.description = DBRequest.Description;
                transactResponse.referenceNumber = DBRequest.ReferenceNumber;
                transactResponse.servicecharge = DBRequest.ServiceCharge;
                transactResponse.serviceref = DBRequest.ServiceTransactionId;
                transactResponse.IsSuccessful = (apiResponse.code == "TS") ? true : false;
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Transact Response: " + JsonConvert.SerializeObject(transactResponse), false);
                return transactResponse;
            }
            catch (Exception ex)
            {
                transactResponse.statusid = "95";
                transactResponse.description = "Transaction Failed. Please contact ECPAY.";
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " Transact Exception Error Response: " + ex.ToString(), false);
                return transactResponse;
            }
        }

        [WebMethod(Description = "API - Check status of Reference number")]
        public CheckPayment CheckPaymentviaRefNumber(string ReferenceNumber)
        {
            //string sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
            CheckPayment dto = new CheckPayment();
            string tokenResponse = string.Empty;
            string sessionId = new SessionIDManager().CreateSessionID(HttpContext.Current);
            try
            {
                Tools.TLS();
                string clientID = ConfigurationManager.AppSettings["UB_client_id"].ToString();
                string secretKey = ConfigurationManager.AppSettings["UB_secret_key"].ToString();
                string partnerID = ConfigurationManager.AppSettings["UB_partner_id"].ToString();
                string CheckPaymentUrl = ConfigurationManager.AppSettings["UB_FundtransferUrl"].ToString();
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " CheckPayment Request: " + ReferenceNumber, false);

                var client = new RestClient(CheckPaymentUrl + "/" +ReferenceNumber);
                var request = new RestRequest(Method.GET);
                request.AddHeader("accept", "application/json");
                request.AddHeader("content-type", "application/json");
                request.AddHeader("x-ibm-client-id", clientID);
                request.AddHeader("x-ibm-client-secret", secretKey);
                request.AddHeader("x-partner-id", partnerID);
                IRestResponse response = client.Execute(request);
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " CheckPayment Response: " + response.Content, false);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    dto = JsonConvert.DeserializeObject<CheckPayment>(response.Content);
                    return dto;
                }
                else
                {
                    UnionbankError errordto = new UnionbankError();
                    //Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " CheckPayment Error Response: " + response.Content, false);
                    errordto = JsonConvert.DeserializeObject<UnionbankError>(response.Content);
                    string code = (errordto.httpCode == null) ? errordto.errors[0].code : errordto.httpCode;
                    string description = (errordto.moreInformation == null) ? (errordto.errors[0].message == null) ? errordto.errors[0].description : errordto.errors[0].message : errordto.moreInformation;
                    dto.record = new Record();
                    dto.record.code = code;
                    dto.record.description = description;
                    return dto;
                }

            }
            catch (WebException webEx)
            {
                using (WebResponse response = webEx.Response)
                {
                    var httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    {
                        UnionbankError errordto = new UnionbankError();
                        StreamReader sr = new StreamReader(data);
                        string res = sr.ReadToEnd();
                        Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate WebException Error Response: " + res.ToString(), false);
                        errordto = JsonConvert.DeserializeObject<UnionbankError>(res);
                        string code = (errordto.httpCode == null) ? errordto.errors[0].code : errordto.httpCode;
                        string description = (errordto.moreInformation == null) ? (errordto.errors[0].message == null) ? errordto.errors[0].description : errordto.errors[0].message : errordto.moreInformation;
                        dto.record = new Record();
                        dto.record.code = code;
                        dto.record.description = description;
                        return dto;
                    }
                }
                //tokenResponse = "95|" + webEx.Message.ToString();
                //Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate WebException Error Response: " + webEx.Message.ToString(), false);
            }
            catch (Exception ex)
            {
                dto.record = new Record();
                dto.record.code = "95";
                dto.record.description = "Transaction Failed. Please contact ECPAY.";
                Tools.LogData(applicationName, "Unionbank", "Unionbank", sessionId + " UnionbankAuthenticate Exception Error Response: " + ex.Message.ToString(), false);
            }
            return dto;
        }
       
    }
}