   M S B A M L   `   `   `  ����  Vuz_Shedule$    Vuz_Shedule.AutorizationWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  Авторизация��5   	    � WindowState$ 	Maximized=�5   	   $
��1280��5	   	   $	��720��5
   	   $��Bookman Old Style)�5   	    � WindowStartupLocation$ CenterScreen=�5   	   ��5      �� 5   
    �� ImageSource$0 *https://i.postimg.cc/9QmW3vJC/image-50.png��6    j� Stretch$ UniformToFill=�5       �� Opacity$
 0.95Q�5      .��5      �� $��40q�6    �� CornerRadius$ 20��5         BackgroundColor#��[  5        SecondaryColor#��[ 5      $��2q�5       J� Effect 5   
   3  -System.Windows.Media.Effects.DropShadowEffect  5         
BlurRadius$ 20Q�6     SecondaryColorValue	   Color#	 [ 5      
   Opacity$	
 0.2Q�5      ._�5   
   �  � RowDefinitions 5       �� 5!      $
��Auto �6    �� 5"      $��* �6    .+�5&      �� -   5         ��Name$ 
BackButton��5&        RoleButtonStyle#��[ 5'      $��60��5(      $��60��5)      $��20q�5*      $
��Left=�5+      .��5-      �� $	��←��6   $��24(�5.        PrimaryColor#��[ 5/      50      �� 53      $��1��6   $ 15��54        LightBackgroundColor#��[ 55      #��[ 56      $	��1.5q�57      $	��400��58      $��20q�59      $��Center=�5:      $��Center=�5;       5<        5=      $ 15Q�6'   #	 [ 5>   %   $	
 0.2Q�5?   %   ._�5B      �� $��30q�6   $��Center=�6)   .+�5C      �� $��Авторизация��6    $��28(�5D      $
��Bold"�5E      #��[ 5F      $��Center=�5G      $��0,0,0,30q�5H      �� 5J      $��Email��6    $��16(�5K      #��[ 5L      $��0,0,0,5q�5M      �� 5N      -   5         ��Name$ EmailTextBox��5N        AuthTextBoxStyle#��[ 5O      
 � Tag$ Email��5P      �� 5R      $��Пароль��6    $��16(�5S      #��[ 5T      $��0,20,0,5q�5U      G� 5V      -   5         G�Name$ PasswordBox��5V   "     AuthPasswordBoxStyle#��[ 5W   !   $ Пароль��5X   !   �� 5Z      -   5        $��
Войти��5Z      #��[ 5[      $
��0,20q�5]      �� 5_      -   5         ��Name$ RegisterLink��5_       $*��$Зарегистрироваться��5`       $��Center=�5a       #��[ 5b        � Cursor$
 Hand��5c       $��	Underline{�5d       5e       5f      5g      5h      5i      5j      