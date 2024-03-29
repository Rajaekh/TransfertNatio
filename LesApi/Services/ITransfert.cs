﻿using LesApi.Models;

namespace LesApi.Services
{
    public interface ITransfert
    {
        Transfert AddTransfert(Transfert transfert);
        bool DepasseMontantAnnuel(string idClient, DateTime dateActuelle, double montantTransfert, double PlafondAnnuel);
        List<Transfert> GetallTransfert();
        List<Transfert> GetAllTransfertsOfClient(string idClient);
        Transfert GetTransfertById(string id);
        Transfert EditTransfertStatus(string id, TransferModel trans);
        Transfert GetTransfertReference(string reference);
        public string GenererNumeroUnique();
        public string GenerateHtmlContent(Transfert transferModel);
    }
}
