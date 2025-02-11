using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace Vuz_Shedule
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AnimateCardAppearance(TeacherCard, 0);
            AnimateCardAppearance(StudentCard, 0.2);
            AnimateCardAppearance(AdminCard, 0.4);
        }

        private void AnimateCardAppearance(FrameworkElement card, double delay)
        {
            card.Opacity = 0;
            card.RenderTransform = new TranslateTransform(0, 50);

            var opacityAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            var translateAnimation = new DoubleAnimation
            {
                From = 50,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delay),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };

            card.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            ((TranslateTransform)card.RenderTransform).BeginAnimation(TranslateTransform.YProperty, translateAnimation);
        }

        private void TeacherButton_Click(object sender, RoutedEventArgs e)
        {
            AutorizationWindow authWindow = new AutorizationWindow("Преподаватель");
            authWindow.Show();
            this.Close();
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            StudentWindow studentWindow = new StudentWindow();
            studentWindow.Show();
            this.Close();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            AutorizationWindow authWindow = new AutorizationWindow("Администратор");
            authWindow.Show();
            this.Close();
        }
    }
}
