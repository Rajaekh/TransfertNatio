﻿using Amazon.Runtime.Internal.Auth;
using LesApi.Models;
using LesApi.Services;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using System.Collections.Generic;
using System.Security.Cryptography.Xml;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransfertController : ControllerBase
    {
        private readonly ITransfert _transfert;
        private readonly IUser _user;
        private readonly IFrais _frais;
        private readonly IBeneficiaire _beneficiaire;
        private readonly IWebHostEnvironment environment;


        public TransfertController(ITransfert transfert, IUser user, IFrais frais, IBeneficiaire beneficiaire, IWebHostEnvironment webHostEnvironment)
        {
            _transfert = transfert;
            _user = user;
            _frais = frais;
            _beneficiaire = beneficiaire;
           
            this.environment = webHostEnvironment;
        }
        // recupérer le liste des transferts:
        [HttpGet]
        public ActionResult <List<TransferModel>> GetallTransfert()
        {
            List <Transfert> trans= _transfert.GetallTransfert();
            List<TransferModel> transferModels = trans.Select(t => new TransferModel
            {
                id = t.Id,
                Notified = t.Notified,
                TypeTransfert = t.TypeTransfert,
                Frais = t.Frais,
                ValFrais = t.ValFrais,
                MotifsTransfert = t.MotifsTransfert,
                DataeTransfert = t.DataeTransfert,
                Montant = t.Montant,
                NomBeneficiaire = GetBeneficiaireFullName(t.IdBeneficiaire),
                NomClient = GetUserFullName(t.IdClient),
                Idagent = t.Idagent,
                Status = t.Status,
                IdClient = t.IdClient,
                IdBeneficiaire = t.IdBeneficiaire,
                otp = t.oTP,
                Reference = t.Reference,
                

            }).ToList();

           


            return transferModels;
        }
        // Helper method to get beneficiary full name
       
        [HttpGet("{reference}")]
        public ActionResult<TransferModel> GetTransfertByReference(string reference)
        {
            var transfert = _transfert.GetTransfertReference(reference);
            if (transfert == null)
            {
                return NotFound(new { error = "Transfert not found." });
            }

            var transferModel = new TransferModel
            {
                Notified = transfert.Notified,
                id = transfert.Id,
                TypeTransfert = transfert.TypeTransfert,
                Frais = transfert.Frais,
                MotifsTransfert = transfert.MotifsTransfert,
                DataeTransfert = transfert.DataeTransfert,
                Montant = transfert.Montant,
                NomBeneficiaire = GetBeneficiaireFullName(transfert.IdBeneficiaire),
                NomClient = GetUserFullName(transfert.IdClient),
                Idagent = transfert.Idagent,
                Status = transfert.Status,
                IdClient = transfert.IdClient,
                IdBeneficiaire = transfert.IdBeneficiaire,
                otp = transfert.oTP,
                ValFrais = transfert.ValFrais,
                Reference=transfert.Reference,
            };

            return Ok(transferModel);
        }
        [HttpPut("{id}")]
        public ActionResult<TransferModel> EditTransfertStatus(string id, TransferModel trans)
        {
            // Vérifiez si les paramètres nécessaires sont présents
            if (id == null)
            {
                return BadRequest("ID or Status is missing.");
            }
           

            var updatedTransfert = _transfert.EditTransfertStatus(id,trans);

            if (updatedTransfert != null)
            {
                var transferModel = new TransferModel
                {
                    Notified = updatedTransfert.Notified,
                    id = updatedTransfert.Id,
                    TypeTransfert = updatedTransfert.TypeTransfert,
                    Frais = updatedTransfert.Frais,
                    MotifsTransfert = updatedTransfert.MotifsTransfert,
                    DataeTransfert = updatedTransfert.DataeTransfert,
                    Montant = updatedTransfert.Montant,
                    NomBeneficiaire = GetBeneficiaireFullName(updatedTransfert.IdBeneficiaire),
                    NomClient = GetUserFullName(updatedTransfert.IdClient),
                    Idagent = updatedTransfert.Idagent,
                    Status = updatedTransfert.Status,
                    IdClient = updatedTransfert.IdClient,
                    IdBeneficiaire = updatedTransfert.IdBeneficiaire,
                    otp = updatedTransfert.oTP,
                    ValFrais= updatedTransfert.ValFrais,
                };
                return Ok(transferModel);
            }
            else
            {
                return NotFound(); // Ou une autre réponse appropriée
            }
        }

        // POST api/<TransfertController>
        [HttpPost]
        public ActionResult<Transfert> Post([FromBody] Transfert transfert)
        {
            // Vérification du Montant
            if (transfert != null && transfert.IdClient != null && transfert.Idagent!=null)
            {
                // ici la date d expiration c est la date de transfert +30j juste un ex

                transfert.DataeExpiration = transfert.DataeTransfert.AddDays(30);
                transfert.Reference = _transfert.GenererNumeroUnique();
                // ici pour stocker la valeur des frais 
                transfert.ValFrais = _frais.CalculerFrais(transfert.Montant, transfert.Frais, transfert.Notified) - transfert.Montant;
                //transfert.Montant = _frais.CalculerFrais(transfert.Montant, transfert.Frais, transfert.Notified);
                var MontantTotal= _frais.CalculerFrais(transfert.Montant, transfert.Frais, transfert.Notified);

                var user = _user.GetUserById(transfert.IdClient);

                if (transfert != null && transfert.TypeTransfert.Equals("En espèce") && user.role.Equals("AGENT"))
                {
                    transfert.IdClient = transfert.Idagent;
                    transfert.PlafondMaximal = 80000;
                }

                // Vérification si le montant du transfert dépasse le plafond annuel
                bool depasseAnnuel = _transfert.DepasseMontantAnnuel(transfert.IdClient, transfert.DataeTransfert, transfert.Montant, transfert.PlafondAnnuel);

                // Ajoutez la condition pour vérifier si le transfert dépasse le montant annuel autorisé
                // condition annuel pour client seulemet
                if (depasseAnnuel &&  user.role.Equals("CLIENT"))
                {
                    return BadRequest(new { error = "Le transfert ne peut pas être effectué : le montant annuel autorisé serait dépassé." });
                }
                else if (MontantTotal > transfert.PlafondMaximal)
                {
                    return BadRequest(new { error = $"Le transfert ne peut pas être effectué : le montant du transfert > plafond maximal du transfert {transfert.PlafondMaximal}" });
                }
                else if (MontantTotal > user.montant)
                {
                    return BadRequest(new { error = "Le transfert ne peut pas être effectué : le montant du transfert > solde de compte de paiement du client." });
                }

                // Soustraction du montant du transfert du solde du compte utilisateur
                user.montant -= MontantTotal;

                // Mise à jour de l'utilisateur dans la base de données
                _user.EditUser(user);
                _transfert.AddTransfert(transfert);
                return Ok(new { id = transfert.Id });
            }

            return BadRequest(new { error = "Les informations de transfert sont invalides." });
        }
        // pour PDF 
        [HttpGet("generatepdf")]
        public async Task<IActionResult> GeneratePDF(string transfertId)
        {
           
            var transfert = _transfert.GetTransfertById(transfertId);

            if (transfert == null)
            {
                return NotFound(new { error = "Transfert not found." });
            }
            var document = new PdfDocument();
            string htmlContent = _transfert.GenerateHtmlContent(transfert);

            PdfGenerator.AddPdfPages(document, htmlContent, PageSize.A4);

            byte[] response;
            using (MemoryStream ms = new MemoryStream())
            {
                document.Save(ms);
                response = ms.ToArray();
            }

            string filename = $"reçuTransfert_{transfertId}.pdf";
            return File(response, "application/pdf", filename);
        }
        string GetBeneficiaireFullName(string beneficiaireId)
        {
            var beneficiaire = _beneficiaire.GetBeneficiaireById(beneficiaireId);
            return beneficiaire != null ? beneficiaire.prenom + " " + beneficiaire.nom : "N/A";
        }

        // Helper method to get user full name
        string GetUserFullName(string userId)
        {
            var user = _user.GetUserById(userId);
            return user != null ? user.lastname + " " + user.name : "N/A";
        }

          [HttpGet("getAllTrasfert/{idClient}")]
          public ActionResult<List<Transfert>> GetAllTrasfertOfClient(string idClient)
                {

            if (idClient == null)
            {
                return BadRequest();
            }
            return _transfert.GetAllTransfertsOfClient(idClient);
                }

    }
}
