using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace WSUnionbank
{
    public class UnionBankDAO
    {
        private string _cnnStr = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString.ToString();
        private SqlConnection _cnn;
        public SqlCommand _cmd;
        public bool Connect()
        {
            if (_cnn != null) Closed();

            bool returnVal;
            try
            {
                _cnn = new SqlConnection();
                _cnn.ConnectionString = _cnnStr;
                _cnn.Open();

                _cmd = new SqlCommand();
                _cmd.CommandType = CommandType.StoredProcedure;
                _cmd.Connection = _cnn;
                returnVal = true;
            }
            catch (System.Exception e)
            {

                throw e;              
            }
            return returnVal;
        }

        public void Closed()
        {
            if (_cmd != null)
            {
                _cmd.Dispose();
            }

            if (_cnn != null)
            {
                _cnn.Close();
                _cnn.Dispose();
            }
        }

        public SqlDataReader ExecuteReader(CommandType commandType
                                         , string commandText
                                         , SqlParameter[] parameter)
        {
            SqlDataReader returnVal = null;

            Connect();
            _cmd.CommandText = commandText;
            _cmd.CommandType = commandType;
            _cmd.Parameters.AddRange(parameter);

            try
            {
                returnVal = _cmd.ExecuteReader();
            }
            catch (SqlException e)
            {
                throw e;  //MessageBox.Show(e.Message, "Error!");
            }

            //dbClosed();
            return returnVal;
        }


        public void ExecuteNonQuery(CommandType commandType
                                  , string commandText
                                  , SqlParameter[] parameters)
        {
            Connect();

            _cmd.CommandText = commandText;
            _cmd.CommandType = commandType;
            //Command.CommandTimeout = 10000;

            for (int i = 0; i < parameters.Length; i++)
            {
                _cmd.Parameters.Add(parameters[i]);
            }

            try
            {
                _cmd.ExecuteNonQuery();
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                Closed();
            }
        }

        public ECashDTO InsertECashTransaction(ECashDTO dto, string applicationName)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@accountId", dto.AccountId)
                , new SqlParameter("@branchId", dto.BranchId)
                , new SqlParameter("@userId", dto.UserId)
                , new SqlParameter("@username", dto.Username)
                , new SqlParameter("@password", dto.Password)
                , new SqlParameter("@accountNo", dto.AccountNumber)
                , new SqlParameter("@amount", dto.Amount)
                , new SqlParameter("@mobileNo", dto.MobileNumber)
                , new SqlParameter("@transferFee", dto.TransferFee)
                , new SqlParameter("@customerName", dto.CustomerName)
                , new SqlParameter("@serviceName", dto.ServiceName)
            };

            try
            {
                using (SqlDataReader dtRead = this.ExecuteReader(CommandType.StoredProcedure
                                                             , "usp_InsertECashTrans"
                                                             , parameters))
                {
                    if (dtRead.Read())
                    {
                        ECashDTO dtoECash = MakeInsertECashResponseProfile(dtRead);
                        this.Closed();
                        return dtoECash;
                    }
                    else
                    {
                        this.Closed();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Tools.LogData(applicationName
                                     , dto.ServiceName
                                     , dto.UserId.ToString()
                                     , "ECashDAO InsertECashTransaction Error: "
                                     + ex.ToString().ToUpper() + Environment.NewLine
                                     + "Function parameters : "
                                     + dto.AccountId.ToString() + Environment.NewLine
                                     + dto.BranchId.ToString() + Environment.NewLine
                                     + dto.UserId.ToString() + Environment.NewLine
                                     + dto.Username + Environment.NewLine
                                     + dto.AccountNumber + Environment.NewLine
                                     + dto.Amount.ToString() + Environment.NewLine
                                     + dto.MobileNumber + Environment.NewLine
                                     + dto.TransferFee.ToString() + Environment.NewLine
                                     + dto.CustomerName.ToString() + Environment.NewLine
                                     + dto.ServiceName.ToString()
                                     , false);

                ECashDTO dtoEx = new ECashDTO();

                dtoEx.StatusId = 92;
                dtoEx.IsSuccessful = false;
                dtoEx.Description = "Transaction failed. Please try again.";

                return dtoEx;
            }
        }

        public void UpdateECashTrans(ECashDTO dto, string applicationName)
        {
            SqlParameter[] stpParameter = new SqlParameter[]
            {
                new SqlParameter("@accountId", dto.AccountId)
                , new SqlParameter("@userId", dto.UserId)
                , new SqlParameter("@amount", dto.FaceCost)
                , new SqlParameter("@referenceNo", dto.ReferenceNumber)
                , new SqlParameter("@serviceName", dto.ServiceName)
                , new SqlParameter("@eCashId", dto.ECashId)
                , new SqlParameter("@statusId", dto.StatusId)
                , new SqlParameter("@remarks", dto.Description)
                , new SqlParameter("@serviceRef", dto.ServiceTransactionId)
            };
            try
            {
                this.ExecuteNonQuery(CommandType.StoredProcedure, "usp_UpdateECashTrans", stpParameter);
                this.Closed();
            }
            catch (Exception ex)
            {
                Tools.LogData(applicationName
                                      , dto.ServiceName
                                      , dto.UserId.ToString()
                                      , "ECashDAO InsertECashTransaction Error: "
                                      + ex.ToString().ToUpper() + Environment.NewLine
                                      + "Function parameters : "
                                      + dto.AccountId.ToString() + Environment.NewLine
                                      + dto.UserId.ToString() + Environment.NewLine
                                      + dto.Amount.ToString() + Environment.NewLine
                                      + dto.ReferenceNumber + Environment.NewLine
                                      + dto.ServiceName + Environment.NewLine
                                      + dto.ECashId.ToString() + Environment.NewLine
                                      + dto.StatusId.ToString() + Environment.NewLine
                                      + dto.Description + Environment.NewLine
                                      , false);
            }
        }

        private ECashDTO MakeInsertECashResponseProfile(SqlDataReader dataReader)
        {
            ECashDTO dto = new ECashDTO();
            Tools.SmartReader1 rdr1 = new Tools.SmartReader1(dataReader);
            dto.IsSuccessful = rdr1.GetBoolean("IsInserted");

            if (dto.IsSuccessful)
            {
                dto.ReferenceNumber = rdr1.GetString("Remarks");
                dto.FaceCost = rdr1.GetDecimal("FaceCost");
                dto.ECashId = rdr1.GetInt64("ECashId");
                dto.ServiceCharge = rdr1.GetDecimal("ServiceCharge");
            }
            else
            {
                dto.Description = rdr1.GetString("Remarks");
            }

            return dto;
        }
   
    
    }
  
}