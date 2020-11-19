﻿using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using VeciHelpAPK.Interface;
using VeciHelpAPK.Models;
using VeciHelpAPK.Security;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace VeciHelpAPK.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NotificacionSospechaView : ContentPage
    {

        public string direccionBase = "http://201.238.247.59/vecihelp/api/v1/";
        Alerta alerta = new Alerta();

        public NotificacionSospechaView(int idAlerta)
        {
            InitializeComponent();
            LblDetalle.IsVisible = false;
            ActualizarAlerta(idAlerta);
        }

        private async void ButtonAcudir_Clicked(object sender, EventArgs e)
        {
            var IdUsuario = int.Parse(Preferences.Get("Ses_id_Usuario", null));
            var token = Preferences.Get("Ses_token", null);

            var endPoint = RestService.For<IAlertas>(new HttpClient(new AuthenticatedHttpClientHandler(token)) { BaseAddress = new Uri(direccionBase) });
            RequestAlerta aler = new RequestAlerta();

            aler.idUsuario = IdUsuario;
            aler.idAlerta = alerta.idAlerta;

            if (alerta.opcionBoton == "Finalizar")
            {
                var response = await endPoint.FinalizarAlerta(aler);
                await DisplayAlert("MUCHAS GRACIAS !!!", response, "Aceptar");

                //elimino 2 ventanas

                Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                await Navigation.PopAsync();

                GlobalClass.varGlobal = false;
            }
            else if (alerta.opcionBoton == "Acudir")
            {
                var response = await endPoint.AcudirAlerta(aler);
                GlobalClass.varGlobal = true;
                response = response.Replace("\"", "");
                if (response.Equals("No puede acudir a mas de una alerta"))
                {
                    await DisplayAlert("Atención", response, "Aceptar");
                }
                else
                {
                    await DisplayAlert("Atención", "Su participación se representa con un ticket en el listado de alertas", "Aceptar");

                    //elimino 2 ventanas

                    Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 2]);
                    await Navigation.PopAsync();

                }
                //await DisplayAlert("Atención", response, "Aceptar");
            }
        }
        private async void ActualizarAlerta(int idAlerta)
        {
            var IdUsuario = int.Parse(Preferences.Get("Ses_id_Usuario", null));
            var token = Preferences.Get("Ses_token", null);

            var endPoint = RestService.For<IAlertas>(new HttpClient(new AuthenticatedHttpClientHandler(token)) { BaseAddress = new Uri(direccionBase) });

            var response = await endPoint.AlertaById(idAlerta, IdUsuario);

            alerta = response;

            LlenarCamposDeAlerta();
        }

        private void LlenarCamposDeAlerta()
        {
          
                LblDetalle.IsVisible = true;
                LblDetalle.Text = alerta.coordenadaSospecha;

            LblNombre.Text = "Informante: "+alerta.nombreAyuda + " " + alerta.apellidoAyuda + "\n" + alerta.direccion;

            LblTipoAlerta.Text = alerta.TipoAlerta.ToUpper();

            LblTipoAlerta.TextColor = Color.FromHex("#2FBB62");


            LblHoraAlerta.Text = "Generada a las " + alerta.horaAlerta.ToString("HH:mm") + " Hrs.";
            LblContadorPersonas.Text = "Esperando ayuda " + alerta.participantes.ToString();

            FotoPerfil.Source = Xamarin.Forms.ImageSource.FromStream(
               () => new MemoryStream(Convert.FromBase64String(alerta.foto)));


            //cambio el boton dependiendo de lo que le corresponda
            if (alerta.opcionBoton == "Ocultar")
            {
                ButtonAcudir.IsVisible = false;
                ButtonAcudir.IsEnabled = false;
            }
            else if (alerta.opcionBoton == "Finalizar")
            {
                ButtonAcudir.BackgroundColor = Color.FromHex("#1d83d4");
                ButtonAcudir.Text = "Ayuda Realizada";
            }

        }

        private void BtnBomberos_Clicked(object sender, EventArgs e)
        {
            PhoneDialer.Open("132");
        }

        private void BtnCarabineros_Clicked(object sender, EventArgs e)
        {
            PhoneDialer.Open(alerta.nroEmergencia);
        }

        private void ButtonSync_Clicked(object sender, EventArgs e)
        {
            ActualizarAlerta(alerta.idAlerta);

        }
    }
}