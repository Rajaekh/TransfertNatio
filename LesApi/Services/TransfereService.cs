using LesApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace LesApi.Services
{
    public class TransfereService : ITransfert
    {
        private readonly IBeneficiaire _IBeneficiaire;
        private readonly IMongoCollection<Transfert> _transfert;
        private readonly IUser _user;
        private readonly IBeneficiaire _beneficiaire;
        public TransfereService(ITransfertDatabaseSettings settings, IMongoClient mongoClient, IUser user, IBeneficiaire beneficiaire)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _user = user;
            _transfert = database.GetCollection<Transfert>(settings.TransfertCollectionName);
            _beneficiaire = beneficiaire;


        }
        public Transfert AddTransfert(Transfert transfert)
        {
            _transfert.InsertOne(transfert);
            return transfert;
        }
        // verifier si le montant de tansfert  depasse le plafond annuel ;
        public bool DepasseMontantAnnuel(string idClient, DateTime dateActuelle, double montantTransfert, double PlafondAnnuel)
        {
            if (idClient != null)
            {
                // Obtenez la date du premier transfert pour le client
                DateTime datePremierTransfert = _user.GetDatePremierTransfert(idClient);

                // Vérifiez si l'utilisateur est trouvé
                var user = _user.GetUserById(idClient);
                if (user != null)
                {
                    // Vérifiez si la date actuelle est après un an à partir de la date du premier transfert
                    if (dateActuelle > datePremierTransfert.AddYears(1))
                    {
                        // Si nous sommes après un an, réinitialisez le montant annuel de transfert
                        user.montantTransfertAnnuel = montantTransfert;
                        // Mettez à jour la date du premier transfert
                        user.datePremierTransfert = dateActuelle;
                        return false;
                    }
                    else
                    {
                        // Si nous sommes toujours dans la même année, vérifiez le montant annuel par rapport au plafond
                        if (user.montantTransfertAnnuel + montantTransfert > PlafondAnnuel)
                        {
                            // Le transfert ne peut pas être effectué car le plafond annuel serait dépassé
                            return true;
                        }
                        else
                        {
                            // Mettez à jour le montant annuel de transfert
                            user.montantTransfertAnnuel += montantTransfert;
                            return false;
                        }
                    }
                }
                else
                {
                    
                    return false;
                }
            }
            else
            {
              
                return false;
            }
        }
        public Transfert EditTransfertStatus(string id, TransferModel trans)
        {
            if (id == null )
            {
                return null; // Les paramètres sont invalides, vous pouvez ajuster cela selon votre logique
            }
            // Obtenez le transfert par ID
            var filter = Builders<Transfert>.Filter.Eq(u => u.Id, id);
            var transfert = _transfert.Find(t => t.Id == id).FirstOrDefault();
            // Vérifiez si le transfert a été trouvé
            if (transfert != null)
            {
                // Mettez à jour le statut
                // on doit mettre a jour le compte de l agent:
                var user = _user.GetUserById(trans.Idagent);
                if (trans.Status.Equals("Extourné") && (transfert.Status.Equals("à servir")))
                {
                    transfert.AutreMotif = trans.AutreMotif;
                    transfert.MotifRestitution = trans.MotifRestitution;
                    transfert.Status = trans.Status;
                    //Modifier l user:
                    user.montant= (double)(transfert.Montant + transfert.ValFrais);
                    _user.EditUser(user);
                }
                else if (trans.Equals("Restitué") && (transfert.Status.Equals("à servir") || transfert.Status.Equals("débloqué à servir")))
                {
                    transfert.AutreMotif = trans.AutreMotif;
                    transfert.MotifRestitution = trans.MotifRestitution;
                    transfert.Status = trans.Status;
                    user.montant = trans.Montant;
                    _user.EditUser(user);
                }
                else if (trans.Equals("Payé") && (transfert.Status.Equals("à servir") || transfert.Status.Equals("débloqué à servir")))
                {
                    transfert.Status = trans.Status;
                }
                
                else if( trans.Equals("Bloqué") || trans.Equals("débloqué à servir"))
                {
                    transfert.Status = trans.Status;
                    transfert.AutreMotif = trans.AutreMotif;
                    transfert.MotifBlicage = trans.MotifBlicage;
                }
                else
                {
                    return null;
                }
               
                // Effectuez la mise à jour dans la base de données
                var result = _transfert.ReplaceOne(filter, transfert);

                // Vérifiez si la mise à jour a réussi
                if (result.ModifiedCount > 0)
                {
                    return transfert; // Retournez le transfert mis à jour
                }
                else
                {
                    return null; // La mise à jour a échoué
                }
            }
            else
            {
                return null; // Le transfert avec l'ID spécifié n'a pas été trouvé
            }
        }
        public List<Transfert> GetallTransfert()
        {
            // Utilisez la méthode Find pour obtenir tous les transferts dans la collection
            var transfertsCursor = _transfert.Find(_ => true);

            // Convertissez les documents en liste de transferts
            List<Transfert> transferts = transfertsCursor.ToList();

            return transferts;
        }
        public Transfert GetTransfertById(string id)
        {
            if (id == null || !IsValidObjectIdFormat(id))
            {
                return null;
            }
            // Utilisez la méthode Find pour obtenir le transfert avec l'ID spécifié
            var transfert = _transfert.Find(t => t.Id == id).FirstOrDefault();

            // Vérifiez si le transfert a été trouvé
            if (transfert != null)
            {
                return transfert;
            }
            else
            {
                // Si le transfert n'est pas trouvé, vous pouvez choisir de lever une exception, de retourner null, ou de gérer d'une autre manière.
                // Dans cet exemple, nous retournons null.
                return null;
            }
        }
        public static bool IsValidObjectIdFormat(string id)
        {
            return ObjectId.TryParse(id, out _);
        }

        // pour generer Reference de Transfert;
        public  string GenererNumeroUnique()
        {
            Random rand = new Random();

            // Générer un nombre aléatoire de 14 chiffres
            int nombreAleatoire = rand.Next(100000000, 999999999);

            // Concaténer avec "EDP837"
            string numeroUnique = "EDP837" + nombreAleatoire.ToString();

            return numeroUnique;
        }

        public Transfert GetTransfertReference(string reference)
        {
            if (reference == null )
            {
                return null;
            }
            // Utilisez la méthode Find pour obtenir le transfert avec l'ID spécifié
            var transfert = _transfert.Find(t => t.Reference == reference).FirstOrDefault();

            // Vérifiez si le transfert a été trouvé
            if (transfert != null)
            {
                return transfert;
            }
            else
            {
                // Si le transfert n'est pas trouvé, vous pouvez choisir de lever une exception, de retourner null, ou de gérer d'une autre manière.
                // Dans cet exemple, nous retournons null.
                return null;
            }
        }
        public string GenerateHtmlContent(Transfert transferModel)
        {
            string receiptTitle = "Reçu de Transfert";
            if (transferModel.Status.Equals("Restitué"))
            {
                receiptTitle = "Reçu de la restitution du transfert";
            }
            else if (transferModel.Status.Equals("Extourné"))
            {
                receiptTitle = "Reçu de l’extourne du transfert ";
            }

            // Générer la chaîne HTML à partir du modèle
            string htmlContent = $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <link href=""https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css"" rel=""stylesheet"">
    
    <style>
       body {{
            font-family: 'Arial', sans-serif;
            margin: 20px;
            padding: 10px;
            background-color: #f4f4f4;
        }}
.center-text {{
    text-align: center;
}}

.right-text {{
    text-align: right;
}}
h1{{
    color: #046B94;
    text-align: center;
    margin-top: 40px; /* Adjust as needed */
    font-weight: bold;
    font-size: 45px;
    font-family: 'Montserrat', sans-serif;

}}
table {{width:100%; 
    border-collapse: collapse;
    margin-bottom: 0; 

}}
 .table-responsive {{margin - top: 20px;
}}

th,
td {{border: 1px solid #0C5973;
    padding: 8px;
    text-align: left;

    }}

th {{background - color: #0C5973;
    color: #000;
}}
.reçu{{
    font-size: 10px;
    padding : 10px;
    }}
.date{{ margin-top:10px;
    font-size: 10px;
    padding : 10px;
    }}
       
    </style>
</head>
<body>
            <div class=""container mt-4"">
                <div class=""row"">
                    <div class=""col-12 center-text"">
                        <h1 class=""font-weight-bold"">TRANSFERT ENSAS</h1>
                        <h2>{receiptTitle}</h2>
                    </div>
                </div>

                <div class=""row mt-3 date"">
                    <div class=""col-6"">
                        <p class=""right-text"">Date d'aujourd'hui: {DateTime.Now.ToString("dd/MM/yy HH:mm")}</p>
                    </div>
                    <div class=""col-6 reçu"">
                        <p class=""right-text"">N° de reçu: {transferModel.Id}</p>
                    </div>
                </div>

                <div class=""table-responsive"">
                    <table class=""table table-bordered table-striped"">
                        <!-- Your table rows here -->
                          <tr>
                              <th>Reference du Transfert</th>
                            <td>{transferModel.Reference}</td>
                        </tr>

                          <tr>
                              <th>Client</th>
                            <td>{GetUserFullName(transferModel.IdClient)}</td>
                        </tr>
                        <tr>
                              <th>Beneficiaire</th>
                            <td>{GetBeneficiaireFullName(transferModel.IdBeneficiaire)}</td>
                        </tr>
                          <tr>
                              <th>ID Agent</th>
                            <td>{transferModel.Idagent}</td>
                        </tr>
                        <tr>
                              <th>Montant</th>
                            <td>{transferModel.Montant} DH</td>
                        </tr>
                     
                            <tr>
                              <th>La Date de Transfert</th>
                            <td>{transferModel.DataeTransfert}</td>
                            </tr>
                              <th>La Date D 'expiration</th>
                            <td>{transferModel.DataeExpiration}</td>
                        </tr>
                        
                          <tr>
                              <th>Frais</th>
                            <td>{transferModel.Frais}</td>
                        </tr>
                        <tr>
                            <th>Valeur des Frais</th>
                            <td>{transferModel.ValFrais}dh</td>
                        </tr>";


            if (transferModel.Status.Equals("Restitué") || transferModel.Status.Equals("Extourné"))
            {
                htmlContent += $@"
                        <tr>
                            <th>Motif de restitution/Extourne</th>
                            <td>{transferModel.MotifRestitution} {transferModel.AutreMotif}</td>
                        </tr>";
            }

            if (transferModel.Status.Equals("Bloqué"))
            {
                htmlContent += $@"
                        <tr>
                            <th>Motif de Bloquage</th>
                            <td>{transferModel.MotifBlicage} {transferModel.AutreMotif}</td>
                        </tr>";
            }

            htmlContent += @"
                    </table>
                </div>
            </div>
            
            <script src=""https://code.jquery.com/jquery-3.3.1.slim.min.js""
                integrity=""sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo""
                crossorigin=""anonymous""></script>
            <script src=""https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.7/umd/popper.min.js""
                integrity=""sha384-UO2eT0CpHqdSJQ6hJty5KVphtPhzWj9WO1clHTMGa3JDZwrnQq4sF86dIHNDz0W1""
                crossorigin=""anonymous""></script>
            <script src=""https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/js/bootstrap.min.js""
                integrity=""sha384-JjSmVgyd0p3pXB1rRibZUAYoIIy6OrQ6VrjIEaFf/nJGzIxFDsf4x0xIM+B07jRM""
                crossorigin=""anonymous""></script>

        </body>";

            return htmlContent;
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

        public List<Transfert> GetAllTransfertsOfClient(string idClient)
        {
            if (idClient == null)
            {              
                return new List<Transfert>();
            }

            // Utilisez la méthode Find pour obtenir tous les transferts avec l'ID spécifié
            var transferts = _transfert.Find(t => t.IdClient == idClient).ToList();

            return transferts;
        }

    }
}
