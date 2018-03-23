
using System;
using System.Collections.Generic;
using System.IO;


namespace BildSql
{
	/// <summary>
	/// Description of MyClass.
	/// </summary>
	/// 
	
 

   
   
	public class MyClass
	{
		public static void Main(string[] args)
		{
			
		
			StreamWriter sw;
				FileInfo fi = new FileInfo(@"c:\sql.txt");
				sw = fi.AppendText();



			TextReader iniFile = null;
			String strLine = null;
			String currentRoot = "";
			
			string iniPath = @"D:\WORK\CS4\UniCS.TM\MID\SQLite.sql";
			if (File.Exists(iniPath))
			{
				try
				{
					iniFile = new StreamReader(iniPath);
					strLine = iniFile.ReadLine();
					while (strLine != null)
					{
						strLine = strLine.Trim();
						if (strLine != "")
						{
							if (strLine.StartsWith("[") && strLine.EndsWith("]"))
							{
								currentRoot=strLine.Substring(1, strLine.Length - 2);
								//sw.WriteLine( currentRoot+"=GetSQL(\""+currentRoot+"\");");	
								sw.WriteLine( "protected string "+ currentRoot + " = @\"\";");									
									
								
							}
							
						}
						strLine = iniFile.ReadLine();
					}
					
						
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					if (iniFile != null)
						iniFile.Close();
				}
			}
			else
				throw new FileNotFoundException("Unable to locate " + iniPath);
			

				
				sw.WriteLine( );
				sw.Flush();
				sw.Close();
				
		}
		
		
	}
}