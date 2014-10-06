using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Timers;
using GenLogic;

namespace GLG
{
    public partial class Form1 : Form
    {
        public GlgChart glg_chart;
        int UpdateInterval = 100;

        public Form1()
        {
            InitializeComponent();

            // Assign GLG drawing name to be displayed in the GLG control.  
            String drawing_name = "chart2.g";
            String drawing_file = Path.Combine( Application.StartupPath, drawing_name );

            // Create a GlgControl, specifying a drawing name to be displayed in the control.
            glg_chart = new GlgChart( drawing_file, true /* prefill chart with data */ );    

            // Add GLG control to the form.
            Controls.Add( glg_chart ); 

            // Position the GLG control.
            ResizeGLGControl();

            // Start periodic updates.
            glg_chart.StartUpdates( UpdateInterval );
        }
        

        //////////////////////////////////////////////////////////////////////
        // Set width and height of the GLG control.
        //////////////////////////////////////////////////////////////////////
        private void ResizeGLGControl()
        {
            // Resize GLG control according to the form's client size.
            glg_chart.Location = new Point(0, 0);
            glg_chart.Size = ClientSize;
        }


        private void GlgRTChartForm_FormClosed( object sender, FormClosedEventArgs e )
        {
            glg_chart.StopUpdates(); 
        }

        private void GlgRTChartForm_Resize( object sender, EventArgs e )
        {
             ResizeGLGControl();
        }
    }
         
    //////////////////////////////////////////////////////////////////////////
    // Extend GlgControl and define a custom class GlgChart.
    //////////////////////////////////////////////////////////////////////////
    public class GlgChart : GlgControl 
    {
        // Number of lines in a chart. May be overriden by the parent object.
        public int NumPlots = 3;   
        
        const double TIME_SPAN = 60.0;          // Initial Time Span in sec.
        
        // Low and High range of the incoming data values.
        double Low = 0.0;
        double High = 10.0;

        private static System.Timers.Timer timer1;

        // Flag to prevent a race condition when the timer is stopped.
        public bool Timer1Enabled = false;
    
        // Update interval in msec.
        int UpdateInterval = 100; 

        double TimeSpan = TIME_SPAN;

        // Store object IDs for each plot. 
        // Used for performance optimization in the chart data feed.
        GlgObject [] Plots; 

        // Number of plots as defined in the drawing.
        int num_plots_drawing;

        Random random = new Random();

        Boolean Ready = false;
   
        // Used in GetCurrTime(). Epoch_time is a DateTime object 
        // representing Epoch time starting January 1, 1970 at 0:0:0,
        // based on UTC.
        //
        static DateTime Epoch_time = 
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Used in GetPlotValue() to generate demo data.
        int counter = 0;

        // Constructor.
        public GlgChart( String drawing_file, Boolean prefill_data )
        {
            // Set DrawingFile property for the GlgControl.
            DrawingFile = drawing_file;

            // Disable automatic update for input events to avoid slowing down 
            // real-time chart updates.
            SetAutoUpdateOnInput( false );
        }

        // HCallback is invoked before the drawing is displayed, and before
        // hierarchy setup.
        public override void HCallback( GlgObject viewport )
        {
            base.HCallback( viewport );

            double major_interval, minor_interval;

            // Retreive number of plots defined in .g file.
            num_plots_drawing = (int) GetDResource( "Chart/NumPlots" );

            // Set new number of plots.
            SetDResource( "Chart/NumPlots", NumPlots );
 
            // Set Time Span for the X axis.
            SetDResource( "Chart/XAxis/Span", TimeSpan );
      
            // Set tick intervals for the Time axis.
            // Use positive values for absolute time interval, for example
            // set major_interval = 10 for a major tick every 10 sec.
            //
            major_interval = -6;      // 6 major intervals
            minor_interval = -5;      // 5 minor intervals
            SetDResource( "Chart/XAxis/MajorInterval", 
                        major_interval );
            SetDResource( "Chart/XAxis/MinorInterval", 
                        minor_interval );
      
            // Set data value range. Since the graph has one Y axis and
            // common data range for the plots, Low/High data range is
            // set on the YAxis level.
            //
            SetDResource( "Chart/YAxis/Low", Low );
            SetDResource( "Chart/YAxis/High", High );
        }
        
        // VCallback is invoked before the drawing is displayed, but after 
        // hierarchy setup.
        public override void VCallback( GlgObject viewport )
        {
            base.VCallback( viewport );

            // Store objects IDs for each plot.
            Plots = new GlgObject[ NumPlots ];
            for( int i=0; i<NumPlots; ++i )
            {
                Plots[i] = viewport.GetNamedPlot( "Chart", "Plot#" + i ); 
            }

            // For the existing plots, use color and line annotation setting 
            // from the drawing; initialize new plots using random colors and strings
            // for demo purposes.
            //
            if( num_plots_drawing < NumPlots )
                for( int i=num_plots_drawing; i < NumPlots; ++i )
                {
                    // Using a random color for a demo.
                    Plots[i].SetGResource( "EdgeColor", random.NextDouble(), 
                        random.NextDouble(), random.NextDouble() );
                                  
                    Plots[i].SetSResource( "Annotation", "Var" + i );
                }
        }

        // ReadyCallback is invoked after the drawing has been displayed for the first time.
        public override void ReadyCallback( GlgObject viewport )
        {
            base.ReadyCallback( viewport );
            Ready = true;
        }

        public void StartUpdates( int interval )
        {
            // Using System.Timers.Timer makes it possible to restart the timer
            // after each update to guarantee an idle interval. This prevents
            // overloading the CPU under heavy loads and on slow machines.

            if( interval !=0 )
                UpdateInterval = interval;
            
            if( timer1 == null )
            {
                // Create a timer.
                timer1 = new System.Timers.Timer( UpdateInterval );
                timer1.AutoReset = false;
                timer1.SynchronizingObject = this;
                // Add timer event handler.
                timer1.Elapsed += new ElapsedEventHandler( OnTimerUpdate );

                Timer1Enabled = true;
                timer1.Start();
            }
        }

        public void StopUpdates()
        {
            if( timer1 != null )
            {
                timer1.Stop();
                timer1 = null;
            }

            Timer1Enabled = false;
        }

        /////////////////////////////////////////////////////////////////////////////
        // Perform periodic dynamic updates.
        /////////////////////////////////////////////////////////////////////////////
        public void OnTimerUpdate( object sender, System.Timers.ElapsedEventArgs e )
        {
            if ( !Timer1Enabled )
                return;

            UpdateChart();

            // Restart the timer.
            timer1.Start();
        }

        //////////////////////////////////////////////////////////////////////////
        // Update the chart with new dynamic data values.
        //////////////////////////////////////////////////////////////////////////
        public void UpdateChart()
        {
            // Perform dynamic updates only if the drawing is ready and 
            // the timer is active.
            if( !Ready )
                return;

            Boolean use_current_time = true; // automatic X axis labeling.
            
            // Supply data to update plot lines. 
            // In this example, current time is automatically supplied
            // by the chart. The application may supply a time stamp instead,
            // by replacing code in GetTimeStamp() method. 
            //
            GetChartData( use_current_time ? 0.0 : GetTimeStamp(), 
                            use_current_time);

            UpdateGlg();
        }

        ///////////////////////////////////////////////////////////////////////
        //  Supplies chart data for each plot.
        ///////////////////////////////////////////////////////////////////////
        public void GetChartData( double time_stamp, Boolean use_current_time )
        {
            double value;
      
            for( int i=0; i < NumPlots; ++i )
            {
                // Get new data value. The example uses simulated data, while
                // an application will replace code in GetPlotValue() to
                // supply application specific data for a given plot index.
                //
                value = GetPlotValue( i );
         
                // Supply plot value for the chart via ValueEntryPoint.
                Plots[ i ].SetDResource( "ValueEntryPoint", value );
                 
                // Supply an optional time stamp. If not supplied, the chart will 
                // automatically generate a time stamp using current time. 
                //
                if( !use_current_time )  
                {   
                    Plots[ i ].SetDResource( "TimeEntryPoint", time_stamp );
                }
         
                // Set ValidEntryPoint resource only if a graph needs to display
                // holes for invalid data points.
                //
                Plots[i].SetDResource( "ValidEntryPoint", 1.0 /*valid*/ );
            }
        }
 
        /////////////////////////////////////////////////////////////////////// 
        // For demo purposes, returns current time in seconds. 
        // Place application specific code here to return a time stamp as needed.
        /////////////////////////////////////////////////////////////////////// 
        double GetTimeStamp( )
        {
            return GetCurrTime(); // for demo purposes.
        }

        /////////////////////////////////////////////////////////////// 
        // Return exact time including fractions of seconds.
        /////////////////////////////////////////////////////////////////////// 
        public double GetCurrTime()
        {
            DateTime time_now = DateTime.UtcNow;
            TimeSpan time_span = time_now - Epoch_time;

            return time_span.TotalSeconds;
        }

        /////////////////////////////////////////////////////////////////////// 
        private void error( String error_str, bool quit )
        {
            Console.WriteLine( error_str );
            if( quit )
                Application.Exit();
        }
          
        /////////////////////////////////////////////////////////////////////// 
        // Generates demo data value. An application can replace code in this
        // method to supply real-time data from a custom data source.
        ///////////////////////////////////////////////////////////////////////
        double GetPlotValue( int plot_index )
        {
            double 
                half_amplitude, center,
                period,
                value,
                alpha;
         
            half_amplitude = ( High - Low ) / 2.0;
            center = Low + half_amplitude;
         
            period = 100.0 * ( 1.0 + plot_index * 2.0 );
            alpha = 2.0 * Math.PI * counter / period;
         
            value = center + 
                half_amplitude * Math.Sin( alpha ) * Math.Sin( alpha / 30.0 );
         
            ++counter;
            return value;
        }
    }
}