   M S B A M L   `   `   `  ����  Vuz_Shedule$    Vuz_Shedule.AutorizationWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	     � Title$  Авторизация��5   	    � WindowState$ 	Maximized=�5   	   $
��1280��5	   	   $	��720��5
   	   $��Bookman Old Style)�5   	    � WindowStartupLocation$ CenterScreen=�5   	   ��5      �� 5   
    �� ImageSource$0 *https://i.postimg.cc/9QmW3vJC/image-50.png��6    j� Stretch$ UniformToFill=�5       �� Opacity$
 0.95Q�5      .��5      �� $��40q�6    �� CornerRadius$ 20��5         BackgroundColor#��[  5        SecondaryColor#��[ 5      $��2q�5       " MaterialDesignElevationShadow2 J� Effect# [ 5      ._�5   
   �  � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��* �6    .+�5"      �� -   5        	 ��Name$	 
BackButton��5"        RoleButtonStyle#��[ 5#      $��60��5$      $��60��5%      $��20q�5&      $
��Left=�5'      .��5)      �� $	��←��6   $��24(�5*        PrimaryColor#��[ 5+      5,      �� 5/      $��1��6   $ 15��50        LightBackgroundColor#��[ 51      #��[ 52      $	��1.5q�53      $	��400��54      $��20q�55      $��Center=�56      $��Center=�57      ._�59      �� $��30q�6   $��Center=�6)   .+�5:      �� $��Авторизация��6    $��28(�5;      $
��Bold"�5<      #��[ 5=      $��Center=�5>      $��0,0,0,30q�5?      �� 5A      -   5        
 ��Name$
 EmailTextBox��5A        AuthTextBoxStyle#��[ 5B      
 � Tag$ Email��5C      G� 5E      -   5         G�Name$ PasswordBox��5E   "     AuthPasswordBoxStyle#��[ 5F   "   $ Пароль��5G   "   �� 5I      -   5        $��
Войти��5I      #��[ 5J      $
��0,20q�5L      �� 5N      -   5         ��Name$ RegisterLink��5N       $*��$Зарегистрироваться��5O       $��Center=�5P       #��[ 5Q        � Cursor$
 Hand��5R       $��	Underline{�5S       5T       5U      5V      5W      5X      5Y      