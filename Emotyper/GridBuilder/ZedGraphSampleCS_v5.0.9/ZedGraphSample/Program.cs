using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ZedGraphSample
{
	static class Program
	{
       
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            MyApplicationContext context = new MyApplicationContext();

            // Run the application with the specific context. It will exit when 
            // all forms are closed.  	
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault( false );		
            Application.Run(context);
		  			
		}
	}


    internal class MyApplicationContext : ApplicationContext
    {
        private int formCount;
        private List<GridVisualiser> forms = new List<GridVisualiser>();
        private GridVisualiser form2;

        private static string[] sensorNames = new string[]
            {"AF3", "F7", "F3", "FC5", "T7", "P7", "O1", "O2", "P8", "T8", "FC6", "F4", "F8", "AF4"};

        public MyApplicationContext()
        {
            formCount = 0;
            int xfactor = 0;
            int yfactor = 0;
            foreach (string sensor in sensorNames)
            {
                var form = new GridVisualiser(sensor);
                form.Closed += new EventHandler(OnFormClosed);
                  form.StartPosition = FormStartPosition.Manual;
               // form.SetBounds(480*xfactor,360*yfactor,480,360);
                form.Location = new Point(480 * xfactor, 360 * yfactor);
                if (xfactor%4==0 && xfactor!=0)
                {
                    xfactor = 0;
                    yfactor++;
                }
                 else
                {
                    xfactor++;
                }
                formCount++;
                forms.Add(form);
            }
            foreach (var form in forms)
            {
                form.Show();
            }
        }

        private void OnFormClosed(object sender, EventArgs e)
        {
            formCount--;
            if (formCount == 0)
                ExitThread();
        }
    }
}