using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GG
{
    class AppTabDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            String templateName;

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                templateName = "AppTabContentTemplate";
            }
            else
            {
                RepositoryViewModel repository = item as RepositoryViewModel;

                templateName = repository.NotOpened ? "AppTabNew" : "AppTabContentTemplate";
            }

            FrameworkElement element = container as FrameworkElement;
            var template = element.TryFindResource(templateName) as DataTemplate;
            if (template != null)
            {
                return template;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
