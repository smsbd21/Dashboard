using System.Data.SqlClient;

namespace DashboardWinApp.DBase
{
    public abstract class DBaseCon
    {
        private readonly string strCon;
        public DBaseCon()
        {
            strCon = "Server=(local); DataBase=NorthwindStore; Integrated Security=true";
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(strCon);
        }
    }
}
