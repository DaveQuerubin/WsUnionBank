using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WSUnionbank
{

    //for body of fundtransfer
    public class AmountBody
    {
        public string currency { get; set; }
        public string value { get; set; }
    }
    public class InfoBody
    {
        public int index { get; set; }
        public string name { get; set; }
        public string value { get; set; }
    }
    public struct FundTransferBody
    {
        public string senderRefId { get; set; }
        public string tranRequestDate { get; set; }
        public string accountNo { get; set; }
        public AmountBody amount { get; set; }
        public string remarks { get; set; }
        public string particulars { get; set; }
        public List<InfoBody> info { get; set; }
    }

    //response from get token
    public class UnionbankAuthenticate
        {
            public string token_type { get; set; }
            public string access_token { get; set; }
            public int expires_in { get; set; }
            public string scope { get; set; }
            public string refresh_token { get; set; }
            public string error { get; set; }
        }
    //response from error
    public class Detail
        {
            public string field { get; set; }
            public string message { get; set; }
        }
    public class Details
    {
        public string field { get; set; }
        public string senderRefId { get; set; }
        public string message { get; set; }
        public string uuid { get; set; }
        public string amount { get; set; }
        public string type { get; set; }
        public DateTime tranRequestDate { get; set; }
        public string tranFinacleDate { get; set; }
        public string coreCode { get; set; }
        }
    public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
            public string description { get; set; }
            public Details detail { get; set; }
            public List<Detail> details { get; set; }
        }
    public class Errors
        {
            public string code { get; set; }
            public string message { get; set; }
            public string description { get; set; }
            public Details details { get; set; }
            //public List<Details> details { get; set; }
        }

    public class UnionbankError
        {
            public List<Error> error { get; set; }
            public List<Errors> errors { get; set; }
            //public string error { get; set; }
            public string httpCode { get; set; }
            public string httpMessage { get; set; }
            public string moreInformation { get; set; }
        }
    //for response from Fundtransfer
    public class FundtransferDTO
        {
            public string code { get; set; }
            public string senderRefId { get; set; }
            public string state { get; set; }
            public string uuid { get; set; }
            public string description { get; set; }
            public string type { get; set; }
            public int amount { get; set; }
            public string ubpTranId { get; set; }
            public DateTime tranRequestDate { get; set; }
            public string tranFinacleDate { get; set; }
            //additional
            //public string signature { get; set; }
            //public string error { get; set; }
            //public string httpCode { get; set; }
            //public string httpMessage { get; set; }
            //public string moreInformation { get; set; }
            //public List<Error> errors { get; set; }
        }
    public class ECashDTO
        {
            public int AccountId { get; set; }
            public int BranchId { get; set; }
            public int UserId { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string AccountNumber { get; set; }
            public decimal Amount { get; set; }
            public string MobileNumber { get; set; }
            public string CustomerName { get; set; }
            public decimal? TransferFee { get; set; }
            public decimal? FaceCost { get; set; }
            public decimal? ServiceCharge { get; set; }
            public string ServiceName { get; set; }
            public bool IsSuccessful { get; set; }
            public string Description { get; set; }
            public string ReferenceNumber { get; set; }
            public Int64 ECashId { get; set; }
            public int StatusId { get; set; }
            public string ServiceTransactionId { get; set; }

            public string LogID { get; set; }
        }
    public class TransactDTO
        {
            public string statusid { get; set; }
            public string description { get; set; }
            public string referenceNumber { get; set; }
            public decimal? servicecharge { get; set; }
            public string serviceref { get; set; }
            public bool IsSuccessful { get; set; }
        }
    //for checkpayment method
    public class Record
        {
            public string senderRefId { get; set; }
            public string code { get; set; }
            public string state { get; set; }
            public string uuid { get; set; }
            public string description { get; set; }
            public string type { get; set; }
            public string amount { get; set; }
            public string ubpTranId { get; set; }
            public DateTime tranRequestDate { get; set; }
            public DateTime createdAt { get; set; }
            public string updatedAt { get; set; }
        }
    public class CheckPayment
        {
            public Record record { get; set; }
        }

}