//using System.Windows;
//using System.Windows.Controls;

//namespace WpfItemsControlSample.ViewModels
//{
//    using Models;

//    public class FigureDataTemplateSelector : DataTemplateSelector
//    {
//        public DataTemplate? LineFigureTemplate      { get; init; }
//        public DataTemplate? EllipseFigureTemplate   { get; init; }
//        public DataTemplate? RectangleFigureTemplate { get; init; }
//        public DataTemplate? PolygonFigureTemplate   { get; init; }

//        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
//            => item switch {
//                  LineFigure      => LineFigureTemplate                  ,
//                  EllipseFigure   => EllipseFigureTemplate               ,
//                  RectangleFigure => RectangleFigureTemplate             ,
//                  PolygonFigure   => PolygonFigureTemplate               ,
//                  _               => base.SelectTemplate(item, container)
//              };
//    }
//}
