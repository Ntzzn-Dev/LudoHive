﻿#pragma checksum "..\..\..\..\..\Telas\Controles\Ordenar.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "7A40D7F7424E9BE98A86946CFDE673385C867698"
//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.42000
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
// </auto-generated>
//------------------------------------------------------------------------------

using LudoHive.Telas.Controles;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LudoHive.Telas.Controles {
    
    
    /// <summary>
    /// Ordenar
    /// </summary>
    public partial class Ordenar : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 227 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid gdOrdenar;
        
        #line default
        #line hidden
        
        
        #line 228 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblElementoPai;
        
        #line default
        #line hidden
        
        
        #line 234 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ScrollBar scrElementos;
        
        #line default
        #line hidden
        
        
        #line 235 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid gdOrdem;
        
        #line default
        #line hidden
        
        
        #line 239 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnRedefinir;
        
        #line default
        #line hidden
        
        
        #line 240 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnSalvar;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.11.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LudoHive;component/telas/controles/ordenar.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.11.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.11.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.gdOrdenar = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.lblElementoPai = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.scrElementos = ((System.Windows.Controls.Primitives.ScrollBar)(target));
            
            #line 234 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
            this.scrElementos.PreviewMouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.Grid_PreviewMouseWheel);
            
            #line default
            #line hidden
            return;
            case 4:
            this.gdOrdem = ((System.Windows.Controls.Grid)(target));
            
            #line 235 "..\..\..\..\..\Telas\Controles\Ordenar.xaml"
            this.gdOrdem.PreviewMouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.Grid_PreviewMouseWheel);
            
            #line default
            #line hidden
            return;
            case 5:
            this.btnRedefinir = ((System.Windows.Controls.Button)(target));
            return;
            case 6:
            this.btnSalvar = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

