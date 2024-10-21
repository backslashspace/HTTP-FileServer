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
			internal static void SendControlPanel(Socket connection, UserDB.User user, String insertInfoString = null)
			{
				String fileContent = Worker.ReadFileText("controlPanel\\controlPanel.html");
				xDebug.WriteLine("controlPanel\\controlPanel.html");

				ControlPanelInsertUsers(ref fileContent, connection);

				fileContent = Regex.Replace(fileContent, "<!-- #DISPLAY#USERNAME#ANCHOR# -->", user.DisplayName);
				fileContent = Regex.Replace(fileContent, "<!-- #THREADPOOL#ANCHOR# -->", $"{ThreadPoolFast.Count}/{ThreadPoolFast.Capacity}");

				if (insertInfoString != null)
				{
					fileContent = Regex.Replace(fileContent, "<!-- #INFO#ANCHOR# -->", insertInfoString);
				}

				Byte[] buffer = Encoding.UTF8.GetBytes(fileContent);

				HTTP.ContentOptions contentOptions = new(HTTP.ContentType.HTML);
				HTTP.HeaderOptions headerOptions = new(HTTP.ResponseType.HTTP_200, contentOptions, (UInt64)buffer.LongLength);
				HTTP.CraftHeader(headerOptions, out Byte[] headerBuffer);

				connection.Send(headerBuffer, 0, headerBuffer.Length, SocketFlags.None);
				connection.Send(buffer, 0, buffer.Length, SocketFlags.None);

				Worker.CloseConnection(connection);
			}
		}

		internal static Boolean ControlPanelInsertUsers(ref String fileContent, Socket connection)
		{
			if (!UserDB.GetDatabaseConnection(out SQLiteConnection databaseConnection))
			{
				Log.FastLog("Database not initialized, this should not happen ", LogSeverity.Critical, "ControlPanel");
				STATIC.Send_500(connection);
				return false;
			};

			SQLiteCommand command = databaseConnection.CreateCommand();
			command.CommandText = "SELECT DisplayName,IsAdministrator,IsEnabled,LoginUsername FROM User WHERE IsAdministrator = 0";
			SQLiteDataReader dataReader = command.ExecuteReader();
			
			if (!dataReader.Read())
			{
				databaseConnection.Close();
				databaseConnection.Dispose();
				return true;
			}

			if (!Tools.CheckFilesDirectory(connection)) return false;
			if (!Tools.GetUserFilesInfo(connection, dataReader.GetString(3), out String fileString)) return false;
			fileContent = Regex.Replace(fileContent, "<!-- #USER#HEAD#ANCHOR# -->", $"<!-- #USER#HEAD#ANCHOR# -->\r\n\r\n\t\t\t<hr style=\"margin-top: 40px;\" />\r\n\r\n\t\t\t<h1>Users</h1>\r\n\r\n\t\t\t<div style=\"display: grid; grid-template-columns: 2fr 1fr 1fr 2fr; gap: 0px; margin-top: 20px;\">\r\n\t\t\t\t<div style=\"text-align: right;\">\r\n\t\t\t\t\t<p>'{dataReader.GetString(0)}'  ({dataReader.GetString(3)})</p>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/config/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">Manage</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/userFiles/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">View Files</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: left;\">\r\n\t\t\t\t\t<p>{fileString}</p>\r\n\t\t\t\t</div>\r\n\t\t\t</div>\r\n\r\n\t\t\t<!-- #USER#ANCHOR# -->\r\n");

			while (dataReader.Read())
			{
				if (!Tools.GetUserFilesInfo(connection, dataReader.GetString(3), out fileString)) return false;
				String userColumn = $"<hr style=\"margin-top: 20px; margin-left: 10em; margin-right: 10em;\" />\r\n\r\n\t\t\t<div style=\"display: grid; grid-template-columns: 2fr 1fr 1fr 2fr; gap: 0px; margin-top: 20px\">\r\n\t\t\t\t<div style=\"text-align: right;\">\r\n\t\t\t\t\t<p>'{dataReader.GetString(0)}'  ({dataReader.GetString(3)})</p>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/config/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">Manage</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: center;\">\r\n\t\t\t\t\t<form method=\"GET\" action=\"/fileSharing/controlPanel/userFiles/loginUsername\">\r\n\t\t\t\t\t\t<button type=\"submit\" style=\"width: 120px; height:28px; font-size: 14px; margin-top: 12px\">View Files</button>\r\n\t\t\t\t\t</form>\r\n\t\t\t\t</div>\r\n\t\t\t\t<div style=\"text-align: left;\">\r\n\t\t\t\t\t<p>{fileString}</p>\r\n\t\t\t\t</div>\r\n\t\t\t</div>\r\n\r\n\t\t\t<!-- #USER#ANCHOR# -->\r\n";
				fileContent = Regex.Replace(fileContent, "<!-- #USER#ANCHOR# -->", userColumn);
			}

			databaseConnection.Close();
			databaseConnection.Dispose();
			return true;
		}
	}
}