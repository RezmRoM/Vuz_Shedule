   M S B A M L   `   `   `  ����  Vuz_Shedule$    Vuz_Shedule.RegistrationWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  Регистрация��5   	    � WindowState$ 	Maximized=�5   	   $
��1280��5	   	   $	��720��5
   	   $��Bookman Old Style)�5   	    � WindowStartupLocation$ CenterScreen=�5   	   ��5      �� 5   
    �� ImageSource$0 *https://i.postimg.cc/9QmW3vJC/image-50.png��6    j� Stretch$ UniformToFill=�6N    �� Opacity$
 0.95Q�6f   .��5      �� $��40q�6    �� CornerRadius$ 20��6      BackgroundColor#��[  6+     SecondaryColor#��[ 5      $��2q�6;    J� Effect 5   
   3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 20Q�6   	   Color#	 [ 6/   
   Opacity$	
 0.2Q�6W   ._�5   
   �  � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��* �6    .+�5      �� -   5         ��Name$ 
BackButton��5        RoleButtonStyle#��[ 5       $��60��5!      $��60��5"      $��20q�5#      $
��Left=�5$      $	��Top=�5%      .��5'      �� $	��←��6   $��24(�6%     PrimaryColor#��[ 63   5(      �� 5+      $��1��6   $
��Auto=�6(   .��5,      �� $ 15��6     LightBackgroundColor#��[ 5-      #��[ 5.      $	��1.5q�5/      $	��500��50      $��20q�51      $��Center=�52      $��Center=�53       54        55      $ 15Q�6+   #	 [ 6;   $	
 0.2Q�6c   ._�58      �� $��30q�6!   $��Center=�6-   .+�59      �� $��Регистрация��6$   $��28(�5:   $   $
��Bold"�5;   $   #��[ 5<   $   $��Center=�5=   $   $��0,0,0,30q�5>   $   �� 5@      -   5         ��Name$ EmailTextBox��5@   "     AuthTextBoxStyle#��[ 5A   "   
 � Tag$ Email��5B   "   �� 5D      -   5        $ LastNameTextBox��5D   "   #��[ 5E   "   $ Фамилия��5F   "   �� 5H      -   5        $ FirstNameTextBox��5H   "   #��[ 5I   "   $ Имя��5J   "   �� 5L      -   5        $ MiddleNameTextBox��5L   "   #��[ 5M   "   $ Отчество��5N   "   G� 5P      -   5         G�Name$ PasswordBox��5P   &     AuthPasswordBoxStyle#��[ 5Q   &   $ Пароль��5R   &   G� 5T      -   5        $ ConfirmPasswordBox��5T   &   #��[ 5U   &   $- 'Подтверждение пароля��5V   &   �� 5X      -   5        $*��$Зарегистрироваться��5X   !   #��[ 5Y   !   $
��0,20q�5[   !   �� 5]      -	   5        $0��*Уже есть аккаунт? Войти��5]   $   $��Center=�5^   $   #��[ 5_   $    � Cursor$
 Hand��5`   $   $��	Underline{�5a   $   5b   $   5c      5d      5e      5f      5g      5h      