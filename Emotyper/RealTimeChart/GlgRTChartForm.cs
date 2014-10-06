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
using Emotiv;
using EmotyperDataUtility;
using GenLogic;
using WaveletStudio.Blocks;

namespace RealTimeChart
{
    public partial class GlgRTChartForm : Form
    {
        public GlgChart glg_chart;
        int UpdateInterval = 100;

        public static Dictionary<EdkDll.EE_DataChannel_t, List<double>> charts =
            new Dictionary<EdkDll.EE_DataChannel_t, List<double>>();

        public GlgRTChartForm()
        {
            InitializeComponent();
            EmotypeEventSource source = new EmotypeEventSource();
            source.OnDataArrived += OnDataArrived;
            source.Start();
            // Assign GLG drawing name to be displayed in the GLG control.  
            String drawing_name = "stripchart_example.g";
            String drawing_file = Path.Combine( Application.StartupPath, drawing_name );

            // Create a GlgViewerControl, specifying a drawing name to be displayed in the control.
            glg_chart = new GlgChart( drawing_file, false /* prefill chart with data */ );    

            // Initialize chart parameters as needed.
            glg_chart.NumPlots = 14;
            glg_chart.Low = 3200.0;
            glg_chart.High = 5600.0;

            // Add GLG control to the form.
            Controls.Add( glg_chart ); 

            // Position the GLG control.
            ResizeGLGControl();

            // Start periodic updates.
            glg_chart.StartUpdates( UpdateInterval );
        }
        void OnDataArrived(object sender, EmoEventDictionary e)
        {

            Dictionary<EdkDll.EE_DataChannel_t, double[]> data = e.Dictionary;
            if (data == null)
                return;
            
            //foreach (EdkDll.EE_DataChannel_t channel in data.Keys)  
            double[] timestamp = data[EdkDll.EE_DataChannel_t.TIMESTAMP];
            for (EdkDll.EE_DataChannel_t k = EdkDll.EE_DataChannel_t.AF3; k <= EdkDll.EE_DataChannel_t.AF4; k++)
            //for (EdkDll.EE_DataChannel_t k = EdkDll.EE_DataChannel_t.AF3; k <= EdkDll.EE_DataChannel_t.FC5; k++)
            {

                List<double> serie = new List<double>(data[k]);
                List<double> seriePrime = processOneSeries(serie);  
                DataPoint temp = new DataPoint();
                foreach (double d in serie)
                {
                   
                    temp.value = d;
                temp.value_valid = true;
         
                temp.has_time_stamp = false; // Use current time stamp for demo
                temp.time_stamp = 0.0;
                      glg_chart.PushPlotPoint(k - EdkDll.EE_DataChannel_t.AF3,temp);
                }
                
               

            }
        }
        private List<double> processOneSeries(List<double> serie)
        {

            //Declaring the blocks
            var inputSeriesBlock = new InputSeriesBlock();
            inputSeriesBlock.SetSeries(serie);
            var dWTBlock = new DWTBlock
            {
                WaveletName = "Daubechies 10 (db10)",
                Level = 1,
                Rescale = false,
                ExtensionMode = WaveletStudio.SignalExtension.ExtensionMode.AntisymmetricWholePoint
            };
            var outputSeriesBlock = new OutputSeriesBlock();

            //Connecting the blocks
            inputSeriesBlock.OutputNodes[0].ConnectTo(dWTBlock.InputNodes[0]);
            dWTBlock.OutputNodes[1].ConnectTo(outputSeriesBlock.InputNodes[0]);

            //Appending the blocks to a block list and execute all
            var blockList = new BlockList();
            blockList.Add(inputSeriesBlock);
            blockList.Add(dWTBlock);
            blockList.Add(outputSeriesBlock);
            blockList.ExecuteAll();
            return outputSeriesBlock.GetSeries();
        }
        //////////////////////////////////////////////////////////////////////////
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
            Environment.Exit(Environment.ExitCode);
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
        public int NumPlots = 14;   
        
        const double TIME_SPAN = 60.0;          // Initial Time Span in sec.
        const double SCROLL_INCREMENT = 10.0;   // Scroll increment in sec.
        
        // Scroll factor for the X axis. 
        double ScrollFactor = SCROLL_INCREMENT / TIME_SPAN;

        const String PLOT_RES_NAME = "ChartViewport/Chart/Plots/Plot#";

        // Low and High range of the incoming data values.
        public double Low = 0.0;
        public double High = 8000.0;

        private static System.Timers.Timer timer1;

        // Flag to prevent a race condition when the timer is stopped.
        public bool Timer1Enabled = false;
    
        // Update interval in msec.
        int UpdateInterval = 100; 

        // Number of samples in the history buffer per line.
        int BufferSize = 5000;
    
        // Setting to False supresses pre-filling chart's buffer with 
        // data on start-up.
        bool PrefillData = true;   

        double TimeSpan = TIME_SPAN;

        // Current auto-scroll state: enabled(1) or disabled(0).
        int AutoScroll = 1;       
  
        // Stored ChartAutoScroll state to be restored if ZoomTo is aborted.
        int StoredScrollState;      

        // Store object IDs for each plot. 
        // Used for performance optimization in the chart data feed.
        GlgObject [] Plots; 

        // Number of plots as defined in the drawing.
        int num_plots_drawing;

        // Used for supplying chart data.
        DataFeedInterface DataFeed;        
         
        // Used by DataFeed to return data values.
         Random random = new Random();

        Boolean Ready = false;
   
        // Used in GetCurrTime(). Epoch_time is a DateTime object 
        // representing Epoch time starting January 1, 1970 at 0:0:0,
        // based on UTC.
        //
        static DateTime Epoch_time = 
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Constructor.
        public GlgChart( String drawing_file, Boolean prefill_data )
        {
            // Activate TraceCallback. By default, it is disabled.
            TraceEnabled = true;

            // Set DrawingFile property for the GlgControl.
            DrawingFile = drawing_file;

            // Defines whether to prefill chart's history buffer with
            // data before the chart starts receiving new data.
            PrefillData = prefill_data;

            // Add DataFeed object used to supply chart data.
            // The example uses demo data. To supply application specific
            // data, replace DemoDataFeed with a custom DataFeed object.
         //   AddDataFeed( new DemoDataFeed( this ) );

            // Disable automatic update for input events to avoid slowing down 
            // real-time chart updates.
            SetAutoUpdateOnInput( false );
        }

        /////////////////////////////////////////////////////////////////////
        // Add DataFeed object for supplying chart data.
        /////////////////////////////////////////////////////////////////////
        public void AddDataFeed( DataFeedInterface data_feed )
        {
            DataFeed = data_feed;
        }

        // HCallback is invoked before the drawing is displayed, and before
        // hierarchy setup.
        public override void HCallback( GlgObject viewport )
        {
            base.HCallback( viewport );

            double major_interval, minor_interval;

            // Retreive number of plots defined in .g file.
            num_plots_drawing = (int) GetDResource( "ChartViewport/Chart/NumPlots" );

            // Set new number of plots.
            SetDResource( "ChartViewport/Chart/NumPlots", NumPlots );
 
            // Set Time Span for the X axis.
            SetDResource( "ChartViewport/Chart/XAxis/Span", TimeSpan );
      
            // Set tick intervals for the Time axis.
            // Use positive values for absolute time interval, for example
            // set major_interval = 10 for a major tick every 10 sec.
            //
            major_interval = -6;      // 6 major intervals
            minor_interval = -5;      // 5 minor intervals
            SetDResource( "ChartViewport/Chart/XAxis/MajorInterval", 
                        major_interval );
            SetDResource( "ChartViewport/Chart/XAxis/MinorInterval", 
                        minor_interval );
      
            // Set data value range. Since the graph has one Y axis and
            // common data range for the plots, Low/High data range is
            // set on the YAxis level.
            //
            SetDResource( "ChartViewport/Chart/YAxis/Low", Low );
            SetDResource( "ChartViewport/Chart/YAxis/High", High );
      
            // Enable AutoScroll, both for the toggle button and the chart.
            ChangeAutoScroll( 1 );

            // Set Chart Zoom mode. It was set and saved with the drawing, 
            // but do it again programmatically just in case.
            //
            viewport.SetZoomMode( "ChartViewport", null, "ChartViewport/Chart", 
                                GlgZoomMode.CHART_ZOOM_MODE );
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
                Plots[i] = viewport.GetNamedPlot( "ChartViewport/Chart", 
                                           "Plot#" + i ); 
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
            
            // Prefill chart's history bufer with data. 
            //if( PrefillData )
            //    PreFillChartData();

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

            // This example uses demo data. An application will provide a
            // custom DataFeed object for supplying real-time chart data.
      
             //Update plot lines with new data supplied by the DataFeed object.
            //for( int i=0; i<NumPlots; ++i )
            //{
            //    // Use DataFeed to get new data value. The DataFeed object
            //    // fills the data_point object with value, time_stamp, etc.
            //    DataFeed.GetPlotPoint( i, data_point );

            //    //String plot_name = PLOT_RES_NAME + i;
            //    PushPlotPoint( i, data_point );
            //}

            UpdateGlg();
        }

        ///////////////////////////////////////////////////////////////////////
        // Pushes the data_point's data into the plot.
        ///////////////////////////////////////////////////////////////////////
        public void PushPlotPoint( int plot_index, DataPoint data_point )
        {
            // Supply plot value for the chart via ValueEntryPoint.
            Plots[ plot_index ].SetDResource( "ValueEntryPoint", data_point.value );
                 
            if( data_point.has_time_stamp )
            {
                // Supply an optional time stamp. If not supplied, the chart will 
                // automatically generate a time stamp using current time. 
                //
                Plots[ plot_index ].SetDResource( "TimeEntryPoint", data_point.time_stamp );
            }
      
            if( !data_point.value_valid )
            {	   
                // If the data point is not valid, set ValidEntryPoint resource to 
                // display holes for invalid data points. If the point is valid,
                // it is automatically set to 1. by the chart.
                //
                Plots[ plot_index ].SetDResource( "ValidEntryPoint", 0.0 );
            }
        }
      
        /////////////////////////////////////////////////////////////////////// 
        // Pre-fill the graph's history buffer with data. 
        /////////////////////////////////////////////////////////////////////// 
        //public void PreFillChartData()
        //{
        //    double 
        //        current_time, start_time, end_time,
        //        num_seconds, dt;

        //    current_time = GetCurrTime();
      
        //    // Roll back by the amount corresponding to the buffer size.
        //    dt = UpdateInterval / 1000.0;     // Update interval is in millisec.
      
        //    // Add an extra second to avoid rounding errors.
        //    num_seconds = BufferSize * dt + 1;
      
        //    start_time = current_time - num_seconds;
        //    end_time = 0.0;   /* Stop at the current time. */

        //    for( int i=0; i<NumPlots; ++i )
        //        DataFeed.FillHistData( i, start_time, end_time, data_point );
        //}
   
        ///////////////////////////////////////////////////////////////////////
        // Handle user interaction.
        ///////////////////////////////////////////////////////////////////////
        public override void InputCallback( GlgObject viewport, GlgObject message_obj )
        {
            String
                origin,
                format,
                action,
                subaction;
      
            origin = message_obj.GetSResource( "Origin" );
            format = message_obj.GetSResource( "Format" );
            action = message_obj.GetSResource( "Action" );
            subaction = message_obj.GetSResource( "SubAction" );
      
            // Process button events.
            if( format.Equals( "Button" ) )   
            {
                if( !action.Equals( "Activate" ) &&      /* Not a push button */
                    !action.Equals( "ValueChanged" ) )   /* Not a toggle button */
                    return;
         
                AbortZoomTo();
         
                if( origin.Equals( "ToggleAutoScroll" ) )
                {         
                    // Set Chart AutoScroll based on the 
                    // ToggleAutoScroll toggle button setting.
                    ChangeAutoScroll( -1 ); 
                }
                else if( origin.Equals( "ZoomTo" ) )
                {
                    // Start ZoomTo operation.
                    SetZoom( "ChartViewport", 't', 0.0 );  
                }
                else if( origin.Equals( "ResetZoom" ) )
                {         
                    // Set initial time span and reset initial Y ranges.
                    SetChartSpan( TimeSpan );  
                    RestoreInitialYRanges();   
                }
                else if( origin.Equals( "ScrollBack" ) )
                {
                    ChangeAutoScroll( 0 );
            
                    // Scroll left. Scrolling can be done by either setting chart's 
                    // XAxis/EndValue resource or by using GlgSetZoom().
            
                    // double end_value;
                    // double end_value = GetDResource( 
                    //     "ChartViewport/Chart/XAxis/EndValue" ); 
                    // end_value -= SCROLL_INCREMENT;
                    // SetDResource( "ChartViewport/Chart/XAxis/EndValue", 
                    //               end_value );
            
                    SetZoom( "ChartViewport", 'l', ScrollFactor );
                 }
                 else if( origin.Equals( "ScrollForward" ) )
                 {
                    ChangeAutoScroll( 0 );
            
                    // Scroll right.
                    SetZoom( "ChartViewport", 'r', ScrollFactor );
                 }
                 else if( origin.Equals( "ScrollToRecent" ) )
                 {
                    // Scroll to show most recent data.
                    ScrollToDataEnd();
                 }
         
                 UpdateGlg();
              }
              else if( format.Equals( "Chart" ) && 
                       action.Equals( "CrossHairUpdate" ) )
              {
                 // To avoid slowing down real-time chart updates, invoke Update() 
                 // to redraw cross-hair only if the chart is not updated fast 
                 // enough by the timer.
                 //
                 if( UpdateInterval > 100 )
                   UpdateGlg();
              }            
              else if( action.Equals( "Zoom" ) )    // Zoom events
              {
                 if( subaction.Equals( "ZoomRectangle" ) )
                 {
                    // Store AutoSCroll state to restore it if ZoomTo is aborted.
                    StoredScrollState = AutoScroll;
            
                    // Stop scrolling when ZoomTo action is started.
                    ChangeAutoScroll( 0 );
                 }
                 else if( subaction.Equals( "End" ) )
                 {
                    // No additional actions on finishing ZoomTo. The Y scrollbar 
                    // appears automatically if needed: it is set to GLG_PAN_Y_AUTO. 
                    // Don't resume scrolling: it'll scroll too fast since we zoomed 
                    // in. Keep it still to allow inspecting zoomed data.
                 }
                 else if( subaction.Equals( "Abort" ) )
                 {
                    // Resume scrolling if it was on.
                    ChangeAutoScroll( StoredScrollState ); 
                 }
         
                 UpdateGlg();
              }
              else if( action.Equals( "Pan" ) )    // Pan events
              {
                 // This code may be used to perform custom action when dragging the 
                 // chart's data with the mouse. 
                 if( subaction.Equals("Start" ) )   // Chart dragging start
                 {
                 }
                 else if( subaction.Equals( "Drag" ) )    // Dragging
                 {
                 }
                 else if( subaction.Equals( "ValueChanged" ) )   // Scrollbars
                 {
                 }
                 /* Dragging ended or aborted. */
                 else if( subaction.Equals( "End" ) || 
                          subaction.Equals( "Abort" ) )
                 {
                 }     
            }   
        }
   
        ///////////////////////////////////////////////////////////////////////
        // Scroll to the end of the data history buffer.
        ///////////////////////////////////////////////////////////////////////
        public void ScrollToDataEnd()
        {
            GlgMinMax min_max = GetViewport().GetDataExtent( "ChartViewport/Chart",  
                                true /* x extent */ );
            if( min_max == null )
                return;
      
            SetDResource( "ChartViewport/Chart/XAxis/EndValue", min_max.max );
        }
   
        ///////////////////////////////////////////////////////////////////////
        // Change chart's AutoScroll mode.
        ///////////////////////////////////////////////////////////////////////
        public void ChangeAutoScroll( int new_value )
        {
            double auto_scroll;
            int pan_x;
      
            if( new_value == -1 )  // Use the state of the ToggleAutoScroll button.
            {
                auto_scroll = GetDResource( "Toolbar/ToggleAutoScroll/OnState" );
                AutoScroll = (int) auto_scroll;
            }
            else    // Set to the supplied value. 
            {
                AutoScroll = new_value;
                SetDResource( "Toolbar/ToggleAutoScroll/OnState", (double) AutoScroll );
            }
      
            // Set chart's auto-scroll.
            SetDResource( "ChartViewport/Chart/AutoScroll", (double) AutoScroll );
      
            // Activate time scrollbar if AutoScroll is Off. The Y value scrollbar 
            // uses GLG_PAN_Y_AUTO and appears automatically as needed.
            //
            pan_x = ( AutoScroll != 0 ? (int) GlgPanType.NO_PAN : (int) GlgPanType.PAN_X );
            SetDResource( "ChartViewport/Pan", (double) ( pan_x | (int) GlgPanType.PAN_Y_AUTO ) );
        }
   
        ///////////////////////////////////////////////////////////////////////
        // Changes the time span shown in the graph.
        ///////////////////////////////////////////////////////////////////////
        public void SetChartSpan( double span )
        {
            if( span > 0 )
                SetDResource( "ChartViewport/Chart/XAxis/Span", span );
            else  // Reset span to show all data accumulated in the buffer.
                SetZoom( "ChartViewport", 'N', 0.0 );
        }    
   
        ///////////////////////////////////////////////////////////////////////
        // Restore Y axis range to the initial Low/High values.
        ///////////////////////////////////////////////////////////////////////
        public void RestoreInitialYRanges()
        {
            SetDResource( "ChartViewport/Chart/YAxis/Low",  Low );
            SetDResource( "ChartViewport/Chart/YAxis/High", High );
        }
   
        ///////////////////////////////////////////////////////////////////////
        // Returns True if the chart's viewport is in ZoomToMode.
        // ZoomToMode is activated on Dragging and ZoomTo operations.
        ///////////////////////////////////////////////////////////////////////
        public Boolean ZoomToMode()
        {
            int zoom_mode = (int) GetDResource( "ChartViewport/ZoomToMode" );
            return ( zoom_mode != 0 );
        }
   
        ///////////////////////////////////////////////////////////////////////
        // Abort ZoomTo mode.
        ///////////////////////////////////////////////////////////////////////
        public void AbortZoomTo()
        {
            if( ZoomToMode() )
            {
                // Abort zoom mode in progress.
                SetZoom( "ChartViewport", 'e', 0.0 ); 
                UpdateGlg();
            }
        }
   
        ///////////////////////////////////////////////////////////////////////
        // Used to obtain coordinates of the mouse click.
        ///////////////////////////////////////////////////////////////////////
        public override void TraceCallback( GlgObject viewport, GlgTraceData trace_info )
        {
            if( !Ready )
                return;
      
            // Process only events that occur in ChartViewport.
            String event_vp_name = trace_info.viewport.GetSResource( "Name" );

            if( !event_vp_name.Equals( "ChartViewport" ) )
                return;
      
            GlgEventType event_type = trace_info.glg_event.GetEventID();
            switch( event_type )
            {
                case GlgEventType.MOUSE_PRESSED:
                case GlgEventType.MOUSE_MOVED:
                    /* Sample code to obtain the mouse coordinates if needed:
                    x = (double) ((MouseEventArgs)trace_info.glg_event.event_args).X;
                    y = (double) ((MouseEventArgs)trace_info.glg_event.event_args).Y;
                    */
                    break;         
                default: return;
            }
      
            switch( event_type )
            {
                case GlgEventType.MOUSE_PRESSED:
                    if( ZoomToMode() )
                        return; // ZoomTo or dragging mode in progress.
         
                    // Start dragging with the mouse on a mouse click. 
                    // If user clicked of an axis, the dragging will be activated in the
                    // direction of that axis. If the user clicked in the chart area,
                    // dragging in both the time and the Y direction will be activated.

                    SetZoom( "ChartViewport", 's', 0.0 );
         
                    // Disable AutoScroll not to interfere with dragging.
                    ChangeAutoScroll( 0 ); 
                    break;
                default: return;
            }
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
        // Sample implementation of a DataFeed interface.
        // In a real application, data will be coming from an application
        // data source defined as the DataFeed object.
        /////////////////////////////////////////////////////////////////////// 
        public class DemoDataFeed : DataFeedInterface
        {
            // Used in GetDemoValue() to generate demo data.
            GlgChart glg_chart;
            long counter = 0;

            public DemoDataFeed( GlgChart chart ) 
            {
                glg_chart = chart;
            }
      
            ///////////////////////////////////////////////////////////
            // Implements DataFeed interface to supply dynamic data.
            // The example uses simulated data by calling GetDemoValue.
            // An application will provide a custom implemnetation of GetPlotPoint()
            // to supply real-time data.
            ///////////////////////////////////////////////////////////////////////
            public void GetPlotPoint( int plot_index, DataPoint data_point )
            {
                data_point.value = GetDemoValue( plot_index );
                data_point.value_valid = true;
         
                data_point.has_time_stamp = false; // Use current time stamp for demo
                data_point.time_stamp = 0.0;
            }
      
            ///////////////////////////////////////////////////////////////////////
            // Implements DataFeed interface to pre-fill the chart's history buffer 
            // with simulated demo data. 
            /////////////////////////////////////////////////////////////////////// 
            public void FillHistData( int plot_index, double start_time, 
                                    double end_time, DataPoint data_point )
            {
               
            }
      
            /////////////////////////////////////////////////////////////////////// 
            // Generates demo data value.
            ///////////////////////////////////////////////////////////////////////
            double GetDemoValue( int plot_index )
            {
                double 
                    half_amplitude, center,
                    period,
                    value,
                    alpha;
         
                double low = glg_chart.Low;
                double high = glg_chart.High;
                half_amplitude = ( high - low ) / 2.0;
                center = low + half_amplitude;
         
                period = 100.0 * ( 1.0 + plot_index * 2.0 );
                alpha = 2.0 * Math.PI * counter / period;
         
                value = center + 
                    half_amplitude * Math.Sin( alpha ) * Math.Sin( alpha / 30.0 );
         
                ++counter;
                return value;
            }
        }
    }
}
