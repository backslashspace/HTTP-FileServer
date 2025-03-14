﻿//using System;
//using System.Data.SQLite;
//using System.Net.Sockets;
//using System.Text;
//using BSS.Threading;
//using System.Web;
//using BSS.Logging;

//namespace Server
//{
//    internal static partial class HTML
//    {
//        internal static partial class CGI
//        {
//            internal static void SendControlPanel(Socket connection, ref readonly UserDB.User user, String insertInfoString = null!, Boolean setSelfURL = false)
//            {
//                Log.Debug("controlPanel\\controlPanel.html", "SendFile()");

//                String fileContent = Worker.ReadFileText("controlPanel\\controlPanel.html");

//                if (!ControlPanelInsertUsers(ref fileContent, connection)) return;

//                fileContent = fileContent.Replace("<!-- #DISPLAY#USERNAME#ANCHOR# -->", user.DisplayName);
//                fileContent = fileContent.Replace("<!-- #THREADPOOL#ANCHOR# -->", $"{ThreadPoolX.Available - ThreadPoolX.Count}/{ThreadPoolX.Count}");

//                if (insertInfoString != null)
//                {
//                    fileContent = fileContent.Replace("<!-- #INFO#ANCHOR# -->", insertInfoString);

//                    fileContent = fileContent.Replace("<!-- #INFO#ANCHOR# -->", insertInfoString);
//                }

//                if (setSelfURL)
//                {
//                    fileContent = fileContent.Replace("<!-- #SCRIPT#ANCHOR# -->", "<script type=\"text/javascript\">\r\n\t\t\twindow.history.replaceState(null, document.title, \"/fileSharing/controlPanel\")\r\n\t\t</script>");
//                }

//                Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

//                HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
//                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
//                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

//                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
//                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

//                Worker.CloseConnection(connection);
//            }
//        }

//        internal static Boolean ControlPanelInsertUsers(ref String fileContent, Socket connection)
//        {
//            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
//            {
//                Log.FastLog("Database not initialized, this should not happen ", LogSeverity.Critical, "ControlPanel");
//                HTTP.ERRORS.Send_500(connection);
//                return false;
//            };

//            SQLiteCommand command = databaseConnection.CreateCommand();
//            command.CommandText = "SELECT DisplayName,IsAdministrator,IsEnabled,LoginUsername FROM User WHERE IsAdministrator = 0";
//            SQLiteDataReader dataReader = command.ExecuteReader();

//            while (dataReader.Read())
//            {
//                if (!Tools.GetUserFilesInfo(connection, dataReader.GetString(3), out String fileString)) return false;
                
//                fileContent = fileContent.Replace("<!-- #USER#ANCHOR# -->", CreateListUser(fileString, dataReader));
//            }

//            databaseConnection.Close();
//            databaseConnection.Dispose();
//            return true;
//        }

//        private static String CreateListUser(String fileString, SQLiteDataReader dataReader) =>
//            $@"<div class=""user"">
//                <span><strong>{HttpUtility.HtmlEncode(dataReader.GetString(0))}</strong> – '{HttpUtility.HtmlEncode(dataReader.GetString(3))}'</span>
//                <form action=""/fileSharing/controlPanel/userSettings"" method=""post"">
//                    <input type=""hidden"" name=""name"" value=""{HttpUtility.HtmlEncode(dataReader.GetString(3))}"">
//                    <button type=""submit"">User Settings</button>
//                </form>
//                <form action=""/fileSharing/files"" method=""post"">
//                    <input type=""hidden"" name=""name"" value=""{HttpUtility.HtmlEncode(dataReader.GetString(3))}"">
//                    <button type=""submit"">View Files</button>
//                </form>
//                <span>{fileString}</span>
//            </div>

//            <!-- #USER#ANCHOR# -->

//            ";
//    }
//}