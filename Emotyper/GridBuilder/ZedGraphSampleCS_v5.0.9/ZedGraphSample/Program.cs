using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

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


    public class MyApplicationContext : ApplicationContext
    {
        private int formCount;
       // public List<GridVisualiser> forms = new List<GridVisualiser>();
        public GridVisualiser form;

        private static string[] sensorNames = new string[]
            {"F7"/*, "F7", "F3", "FC5", "T7", "P7", "O1", "O2", "P8", "T8", "FC6", "F4", "F8", "AF4"*/};

        public MyApplicationContext()
        {
            formCount = 1;
            int xfactor = 0;
            int yfactor = 0;
           
            //foreach (string sensor in sensorNames)
            //{
                 form = new GridVisualiser();
                form.Closed += new EventHandler(OnFormClosed);
            form.Show();
                 // form.StartPosition = FormStartPosition.Manual;
               // form.SetBounds(480*xfactor,360*yfactor,480,360);
            //    form.Location = new Point(480 * xfactor, 360 * yfactor);
            //    if (xfactor%4==0 && xfactor!=0)
            //    {
            //        xfactor = 0;
            //        yfactor++;
            //    }
            //     else
            //    {
            //        xfactor++;
            //    }
            //    formCount++;
            //    forms.Add(form);
            ////}
            //foreach (var form in forms)
            //{
            //    form.Show();
            //}
           
            //Timer timer = new Timer();
            //timer.Tick += new EventHandler(timer_Tick); // Everytime timer ticks, timer_Tick will be called
            //timer.Interval = (1000) * (1);              // Timer will tick evert second
            //timer.Enabled = true;                       // Enable the timer
            //timer.Start();                 
           
        }

      
        //void timer_Tick(object sender, EventArgs e)
        //{
        //    Random r = new Random();
        //    forms[0].updateCurrent(r.NextDouble() * 100, r.NextDouble() * 100);
       // }
        private void OnFormClosed(object sender, EventArgs e)
        {
            formCount--;
            if (formCount == 0)
                ExitThread();
        }
    }
}