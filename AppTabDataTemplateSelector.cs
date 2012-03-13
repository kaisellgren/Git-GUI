using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GG
{
    class AppTabDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object tab, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;
            Repository repository = tab as Repository;

            return element.FindResource(repository.NotOpened ? "AppTabNew" : "AppTabContentTemplate") as DataTemplate;
        }
    }
}
