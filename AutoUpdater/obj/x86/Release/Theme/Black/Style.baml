   M S B A M L   `   `   `  ���� \  XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� 
TargetType �� Property �� Value �� 
TargetType � ColumnDefinitions � RowDefinitions �� CanContentScroll �� Orientation � Minimum	 �� ScrollableHeight
 � Maximum �� VerticalOffset � Value �� ViewportHeight �� ViewportSize* �� #ComputedVerticalScrollBarVisibility �� ScrollableWidth �� HorizontalOffset �� ViewportWidth, �� %ComputedHorizontalScrollBarVisibility   ScrollBar_LineButton F�Name �� Path �� RelativeSource �� Triggers P� Property P� Value �� 
TargetName  ScrollBar_PageButton  ScrollBar_Thumb �� CornerRadius ]� DecreaseRepeatButton ]� Thumb ]� IncreaseRepeatButton�� D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     :x,http://schemas.microsoft.com/winfx/2006/xaml      %�  '��       &	  �    &	 �    &	 �    '�� �    7  7  7  �� "  ��.���  �@�OverridesDefaultStyle . ��  � �� �� " ��.R��  �� $��* ��� $
��Auto � �� $��* ��� $
��Auto �.+��� $��0��$��0��$	��Top=�._���  . #��z���� $��PART_VerticalScrollBar��$ Vertical=�$��0��$��1��$ 0Q�#
 z	 # z # z #y�z �� $��PART_HorizontalScrollBar��$ 
Horizontal=�$��1��$��1��$ 0Q�#
 z # z # z #y�z �� "  ��.���  �@�OverridesDefaultStyle . ��  � �� �� " ��.R�F� $ arrow��$��Center=����#42464d
������ $��Center=����� $ Content� �� *TemplatedParent+ P�  �@J�IsMouseOver . . ��� $ arrow�� � ��
 �����P�  � �� . . ��� $ arrow�� � ��
 ������� "  ��.���  �@�OverridesDefaultStyle . ��  � �� .  ��  � |� .  ��  � �� �� " ��.R��� 
������ �� "  n�.���  � �� �� " n�.R��� $ 3��
������ ���#42464d$��12��$��1q��� "  ��.���  �@��IsFlicksEnabled .  ��  � ��
 ���� ��  � ��$ 14����  � ��$ 14����  � �� �� " ��.R��  �� $��16 ��� $��* ��� $��16 �.+��� $��0��#���   $��ScrollBar.LineUpCommand��$��M 0 10 L 6 0 L 12 10 Z��]� $��
PART_Track��$��1����.  �� 8��   n� 8�� $��4�� �� 8�� �� $��2��#���   $��ScrollBar.LineDownCommand��$��M 0 0 L 6 10 L 12 0 Z��