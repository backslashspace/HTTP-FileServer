using BSS.Logging;
using BSS.Threading;
using System;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System.Web;

namespace Server
{
    internal static partial class Worker
    {
        private static void SendControlPanel(SecureSocket connection, ref readonly UserDB.User invokingUser, String insertInfoString = null!, Boolean setSelfURL = false)
        {
            String fileContent;

            try
            {
                fileContent = File.ReadAllText(Program.AssemblyPath + "html\\fileSharing\\controlPanel\\controlPanel.html");
            }
            catch
            {
                Log.FastLog("Unable to load html\\controlPanel\\controlPanel.html -> 500", LogSeverity.Error, "SendControlPanel");
                HTTP.ERRORS.Send_500(connection);
                connection.Close();
                return;
            }

            if (!ControlPanelInsertUsers(ref fileContent, connection)) return;

            fileContent = fileContent.Replace("<!-- #DISPLAY#USERNAME#ANCHOR# -->", invokingUser.DisplayName);
            fileContent = fileContent.Replace("<!-- #THREADPOOL#ANCHOR# -->", ThreadPoolX.Count - ThreadPoolX.Available + "/" + ThreadPoolX.Count);

            if (insertInfoString != null) fileContent = fileContent.Replace("<!-- #INFO#ANCHOR# -->", insertInfoString);

            if (setSelfURL) fileContent = fileContent.Replace("<!-- #SCRIPT#ANCHOR# -->", "<script type=\"text/javascript\">\r\n\t\t\twindow.history.replaceState(null, document.title, \"/fileSharing/controlPanel\")\r\n\t\t</script>");

            Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

            HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, new(HTTP.ContentType.HTML), buffer.LongLength);
            HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

            connection.SslStream!.Write(headerBuffer, 0, headerBuffer.Length);
            connection.SslStream!.Write(buffer);
            connection.Close();
        }

        private static Boolean ControlPanelInsertUsers(ref String fileContent, SecureSocket connection)
        {
            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
            {
                Log.FastLog("Database not initialized, this should not happen ", LogSeverity.Critical, "ControlPanel");
                HTTP.ERRORS.Send_500(connection);
                return false;
            }

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT DisplayName,IsAdministrator,IsEnabled,LoginUsername FROM User WHERE IsAdministrator = 0";
            SQLiteDataReader dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                if (!GetUserFilesInfo(connection, dataReader.GetString(3), out String fileString))
                {
                    databaseConnection.Close();
                    databaseConnection.Dispose();
                    return false;
                }

                fileContent = fileContent.Replace("<!-- #USER#ANCHOR# -->", CreateListUser(fileString, dataReader));
            }

            databaseConnection.Close();
            databaseConnection.Dispose();
            return true;
        }

        private static String CreateListUser(String fileString, SQLiteDataReader dataReader) =>
            $@"<div class=""user"">
                        <span><strong>{HttpUtility.HtmlEncode(dataReader.GetString(0))}</strong> – '{HttpUtility.HtmlEncode(dataReader.GetString(3))}'</span>
                        <form action=""/fileSharing/controlPanel/userSettings"" method=""post"">
                            <input type=""hidden"" name=""name"" value=""{HttpUtility.HtmlEncode(dataReader.GetString(3))}"">
                            <button type=""submit"">User Settings</button>
                        </form>
                        <form action=""/fileSharing/files"" method=""post"">
                            <input type=""hidden"" name=""name"" value=""{HttpUtility.HtmlEncode(dataReader.GetString(3))}"">
                            <button type=""submit"">View Files</button>
                        </form>
                        <span>{fileString}</span>
                    </div>

                    <!-- #USER#ANCHOR# -->

                    ";
    }
}