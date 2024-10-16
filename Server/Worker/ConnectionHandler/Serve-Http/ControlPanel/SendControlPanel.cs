using System;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using BSS.Logging;
using BSS.Threading;

namespace Server
{
    internal static partial class HTML
    {
        internal static partial class CGI
        {
            internal static void SendControlPanel(Socket connection, UserDB.User user)
            {
                String html = Worker.ReadFileText("controlPanel\\controlPanel.html");
                xDebug.WriteLine("controlPanel\\controlPanel.html");

                ControlPanelInsertUsers(ref html);

                Regex.Replace(html, "<!-- #DISPLAY#USERNAME#ANCHOR# -->", user.DisplayName);
                Regex.Replace(html, "<!-- #THREADPOOL#ANCHOR# -->", $"{ThreadPoolFast.Count}/{ThreadPoolFast.Capacity}");

                Byte[] buffer = Encoding.UTF8.GetBytes(html);

                HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
                HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
                HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

                connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
                connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

                Worker.CloseConnection(connection);
            }
        }

        internal static void ControlPanelInsertUsers(ref String html)
        {
            if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection)) return;

            SQLiteCommand command = databaseConnection.CreateCommand();
            command.CommandText = "SELECT DisplayName,IsAdministrator,IsEnabled,LoginUsername FROM User";
            SQLiteDataReader dataReader = command.ExecuteReader();

            if (!dataReader.Read())
            {
                databaseConnection.Close();
                databaseConnection.Dispose();
                return;
            }

            // skip till user is not admin
            while (dataReader.GetBoolean(1))
            {
                if (!dataReader.Read())
                {
                    databaseConnection.Close();
                    databaseConnection.Dispose();
                    return;
                }
            }

            // insert user headline and first user line + user anchor
            Regex.Replace(html, "<!-- #USER#HEAD#ANCHOR# -->", $"<!-- #USER#HEAD#ANCHOR# -->\r\n\r\n\t\t\t<hr style=\"margin-top: 40px;\" />\r\n\r\n\t\t\t<h1>Users</h1>\r\n\r\n\t\t\t<div style=\"display: grid; grid-template-columns: 2fr 1fr 1fr 2fr; gap: 0px; margin-top: 20px;\">\r\n\t\t\t\t<div style=\"text-align: right;\">\r\n\t\t\t\t\t<p>display username</p>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/config/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">Manage</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/userFiles/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">View Files</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: left;\">\r\n\t\t\t\t\t<p>12GB in 7 files</p>\r\n\t\t\t\t</div>\r\n\t\t\t</div>\r\n\r\n\t\t\t<!-- #USER#ANCHOR# -->\r\n");

            // insert additional users
            while (dataReader.Read())
            {
                Regex.Replace(html, "<!-- #USER#ANCHOR# -->", "<hr />\r\n\r\n\t\t\t<div style=\"display: grid; grid-template-columns: 2fr 1fr 1fr 2fr; gap: 0px; margin-top: 20px\">\r\n\t\t\t\t<div style=\"text-align: right;\">\r\n\t\t\t\t\t<p>display username</p>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/config/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">Manage</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/userFiles/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">View Files</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: left;\">\r\n\t\t\t\t\t<p>12GB in 7 files</p>\r\n\t\t\t\t</div>\r\n\t\t\t</div>\r\n\r\n\t\t\t<!-- #USER#ANCHOR# -->\r\n");
            }

            databaseConnection.Close();
            databaseConnection.Dispose();
            return;
        }
    }
}