﻿using System;

public interface DataFeedInterface
{
   void GetPlotPoint( int plot_index, DataPoint data_point );

   // This method may be used to pre-fill data in the real-time mode.
   // It may do nothing if pre-filling data is not needed.
   void FillHistData( int plot_index, 
                             double start_time, double end_time, 
                             DataPoint data_point );
}
