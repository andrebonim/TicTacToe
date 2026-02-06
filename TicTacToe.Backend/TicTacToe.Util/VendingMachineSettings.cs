using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TicTacToe.Util
{
    public class VendingMachineSettingsModel
    {
        
        public string EmailCliente { get; set; }
        public string ModeloEquipamento { get; set; }
        public string SerieEquipamento { get; set; }
        public string SerialPinpad { get; set; }
        public string PortaPinpad { get; set; }
        public string PortaDispenser { get; set; }
    }

    public static class AppConfig
    {
        private static VendingMachineSettingsModel? _settings;
        private static readonly object _lock = new();

        public static VendingMachineSettingsModel Settings
        {
            get
            {
                if (_settings == null)
                {
                    //throw new InvalidOperationException("Configuração não inicializada. Chame Initialize ou Refresh primeiro.");
                }
                return _settings;
            }
        }   

        // Chama no startup (Program.cs) ou em qualquer lugar
        public static void Initialize(string EmailCliente
            ,string ModeloEquipamento
            ,string SerieEquipamento
            ,string SerialPinpad
            ,string PortaPinpad
            ,string PortaDispenser
            )
        {
            Refresh(EmailCliente, ModeloEquipamento, SerieEquipamento, SerialPinpad, PortaPinpad, PortaDispenser);
        }

        // Método principal para recarregar a qualquer momento
        public static void Refresh(string EmailCliente
            , string ModeloEquipamento
            , string SerieEquipamento
            , string SerialPinpad
            , string PortaPinpad
            , string PortaDispenser)
        {            

            var newSettings = new VendingMachineSettingsModel
            {
                EmailCliente = EmailCliente,
                ModeloEquipamento = ModeloEquipamento,
                SerieEquipamento = SerieEquipamento,
                SerialPinpad = SerialPinpad,
                PortaPinpad = PortaPinpad,
                PortaDispenser = PortaDispenser
                // ... todos os campos
            };

            lock (_lock)  // thread-safe
            {
                _settings = newSettings;
            }
        }
    }
}
