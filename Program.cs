

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Biztalk2013R2PartnerAgreementManager.XmlSettings;

namespace Biztalk2013R2PartnerAgreementManager
{
    class Program
    {
        //enumerate all the TPM Profiles in BizTalk
        static SqlConnectionStringBuilder builder;

        static void Main(string[] args)
        {
            try
            {

                builder =
                new SqlConnectionStringBuilder(@"DATA SOURCE=" + args[0] + ";Initial Catalog=BizTalkMgmtDb;"
            + "Integrated Security=SSPI;MultipleActiveResultSets=True");
      //          builder =
      //    new SqlConnectionStringBuilder(@"DATA SOURCE=amishwin8;Initial Catalog=BizTalkMgmtDb;"
      //+ "Integrated Security=SSPI;MultipleActiveResultSets=True");
                BiztalkPartySettings partySettings = new BiztalkPartySettings();
                var serializer = new XmlSerializer(typeof(BiztalkPartySettings));
                //using (var reader = XmlReader.Create(@"C:\Alok\Code\AmateurSamples\TradingPartnerAgreementManager\BiztalkPartySettings.xml"))
               using (var reader = XmlReader.Create(args[1]))
                {
                    partySettings = (BiztalkPartySettings)serializer.Deserialize(reader);
                }

                TPAM tpm = new TPAM();


                foreach (var party in partySettings.Parties)
                {
                    if (!tpm.VerifyPartnerExists(party.Name, builder))
                    {
                        tpm.CreatePartner(party.Name, builder);
                    }
                    foreach (var sendportRef in party.SendPorts)
                    {
                        if (!tpm.VerifySendPortExistsInPartner(party.Name, sendportRef.Name, builder))
                            tpm.AddSendPortToPartner(party.Name, sendportRef.Name, builder);
                    }

                    foreach (BusinessProfile businessProfile in party.BusinessProfiles)
                    {
                        if (!tpm.VerifyBusinessProfileExists(party.Name, businessProfile.Name, builder))
                        {
                            tpm.AddBusinessProfileToPartner(party.Name, businessProfile.Name, builder);
                        }

                        foreach (BusinessProfileIdentity identity in businessProfile.Identities)
                        {
                            if (!tpm.VerifyIdentityExistsInPartnerBusinessProfile(party.Name, businessProfile.Name, identity.IdentityQualifier + ":" + identity.IdentityValue + ":" + identity.IdentityDesc, builder))
                                tpm.AddIdentityToPartnerBusinessProfile(party.Name, businessProfile.Name, identity.IdentityQualifier + ":" + identity.IdentityValue + ":" + identity.IdentityDesc, builder);
                        }
                        foreach (BusinessProfileAgreement agreement in businessProfile.Agreements)
                        {
                            if (!tpm.VerifyPartnershipExists(agreement.FirstParty.Name, agreement.SecondParty.Name, builder))
                            {
                                tpm.CreatePartnership(agreement.FirstParty.Name, agreement.SecondParty.Name, builder);
                            }

                            if (!tpm.VerifyAgreementExists(agreement.Name, builder))
                            {
                                if (agreement.Protocol == "X12")
                                {
                                    tpm.CreateX12Agreement(agreement.FirstParty.Name + ":" + agreement.FirstParty.BusinessProfileName,
                                        agreement.SecondParty.Name + ":" + agreement.SecondParty.BusinessProfileName, agreement.FirstParty.Identity.IdentityQualifier + ":" + agreement.FirstParty.Identity.IdentityValue,
                                        agreement.SecondParty.Identity.IdentityQualifier + ":" + agreement.SecondParty.Identity.IdentityValue, agreement.Name, builder);




                                }
                                if (agreement.Protocol == "AS2")
                                {
                                    tpm.CreateAS2Agreement(agreement.FirstParty.Name + ":" + agreement.FirstParty.BusinessProfileName,
                                        agreement.SecondParty.Name + ":" + agreement.SecondParty.BusinessProfileName, agreement.FirstParty.Identity.IdentityQualifier + ":" + agreement.FirstParty.Identity.IdentityValue,
                                        agreement.SecondParty.Identity.IdentityQualifier + ":" + agreement.SecondParty.Identity.IdentityValue, agreement.Name, builder);




                                }
                            }
                            if (agreement.FirstPartyToSecondPartyOneWayAgreement != null)
                            {
                                if(agreement.FirstPartyToSecondPartyOneWayAgreement.AgreementAliases!=null && agreement.FirstPartyToSecondPartyOneWayAgreement.AgreementAliases.Count>0)
                                {
                                    foreach(var alias in agreement.FirstPartyToSecondPartyOneWayAgreement.AgreementAliases)
                                    {
                                        if(!tpm.VerifyAgreementAliasExistsForAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, alias.AliasKey,alias.AliasValue, builder))
                                        {
                                            tpm.AddAgreementAliasToAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, alias.AliasKey, alias.AliasValue, builder);
                                        }
                                    }
                                }
                                if (agreement.FirstPartyToSecondPartyOneWayAgreement.SendPorts != null && agreement.FirstPartyToSecondPartyOneWayAgreement.SendPorts.Count>0)
                                {
                                    foreach (var sendPort in agreement.FirstPartyToSecondPartyOneWayAgreement.SendPorts)
                                        if (!tpm.VerifySendPortExistsInOneWayAgreementAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, sendPort.Name, builder))

                                            tpm.AddSendPortToOneWayAgreementAgreement(party.Name, agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, sendPort.Name, builder);
                                }
                                if (agreement.Protocol == "X12" && agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings != null)
                                {
                                    if (agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings.Acknowledgement != null)
                                    {
                                        tpm.EnableFunctionalAckForX12Agreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings.Acknowledgement.Expect997, builder);
                                    }
                                    if (agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators != null)
                                    {
                                        tpm.SetCharacterSetAndSeparatorForX12Agreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name,
                                            agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators.DataElementSeparator + ":" +
                                        agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators.ComponentElementSeparator + ":" +
                                        agreement.FirstPartyToSecondPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators.SegmentTerminator, builder);
                                    }
                                   

                                }

                                if (agreement.Protocol == "X12" && agreement.FirstPartyToSecondPartyOneWayAgreement.Batches != null && agreement.FirstPartyToSecondPartyOneWayAgreement.Batches.Count()>0)
                                {
                                    foreach (var batch in agreement.FirstPartyToSecondPartyOneWayAgreement.Batches)
                                    {
                                        if(!tpm.VerifyBatchExistsForX12OneWayAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name,batch.Name,builder))
                                        {
                                            
                                            tpm.AddBatchForX12OneWayAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, batch, builder);
                                        }
                                    }
                                }
                                if (agreement.Protocol == "X12" && agreement.FirstPartyToSecondPartyOneWayAgreement.TransactionSettings != null)
                                {
                                    if (agreement.FirstPartyToSecondPartyOneWayAgreement.TransactionSettings.Envelopes != null)
                                    {
                                        foreach (var envelope in agreement.FirstPartyToSecondPartyOneWayAgreement.TransactionSettings.Envelopes)
                                        {
                                            string messageid = "", protocolVersion = "", targetNamespace = "";
                                            messageid = envelope.TransactionType;
                                            protocolVersion = envelope.Version;
                                            targetNamespace = envelope.TargetNamespace;
                                            if (!tpm.VerifyEnvelopeExistsForX12OneWayAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, messageid + ":" + protocolVersion, targetNamespace, builder))
                                           
                                            tpm.AddEnvelopesForX12OneWayAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, messageid + ":" + protocolVersion, targetNamespace,
                                                envelope.GS3, envelope.GS2, envelope.GS4 + ":" + envelope.GS5, envelope.GS7, envelope.GS1, builder);
                                        }
                                    }
                                    if (agreement.FirstPartyToSecondPartyOneWayAgreement.TransactionSettings.SchemaOverides != null && agreement.FirstPartyToSecondPartyOneWayAgreement.TransactionSettings.SchemaOverides.Count>0)
                                    {
                                        foreach (var schemaOveride in agreement.FirstPartyToSecondPartyOneWayAgreement.TransactionSettings.SchemaOverides)
                                        {
                                            string messageid = "", protocolVersion = "", targetNamespace = "",senderid="";
                                            messageid = schemaOveride.TransactionType;
                                            protocolVersion = schemaOveride.Version;
                                            targetNamespace = schemaOveride.TargetNamespace;
                                            senderid = schemaOveride.GS2;
                                            if (!tpm.VerifySchemaOverideExistsForX12OneWayAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, messageid + ":" + protocolVersion, senderid, builder))

                                                tpm.AddSchemaOveridesForX12OneWayAgreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, messageid + ":" + protocolVersion, targetNamespace,
                                                     senderid, builder);
                                        }
                                    }
                                }
                                
                                if (agreement.Protocol == "AS2" && agreement.FirstPartyToSecondPartyOneWayAgreement.MDNAcknowledgementSettings != null)
                                {


                                    tpm.EnableMDNForAS2Agreement(agreement.Name, agreement.FirstParty.Name, agreement.SecondParty.Name, agreement.FirstPartyToSecondPartyOneWayAgreement.MDNAcknowledgementSettings.SigningAlgorithm, agreement.FirstPartyToSecondPartyOneWayAgreement.MDNAcknowledgementSettings.ReceiptDeliveryOptionUrl, builder);
                                        
                                    
                                }

                            }

                            if (agreement.SecondPartyToFirstPartyOneWayAgreement != null)
                            {
                                if (agreement.SecondPartyToFirstPartyOneWayAgreement.AgreementAliases != null && agreement.SecondPartyToFirstPartyOneWayAgreement.AgreementAliases.Count > 0)
                                {
                                    foreach (var alias in agreement.SecondPartyToFirstPartyOneWayAgreement.AgreementAliases)
                                    {
                                        if (!tpm.VerifyAgreementAliasExistsForAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, alias.AliasKey, alias.AliasValue, builder))
                                        {
                                            tpm.AddAgreementAliasToAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, alias.AliasKey, alias.AliasValue, builder);
                                        }
                                    }
                                }
                                if (agreement.SecondPartyToFirstPartyOneWayAgreement.SendPorts != null && agreement.SecondPartyToFirstPartyOneWayAgreement.SendPorts.Count>0)
                                {
                                    foreach (var sendPort in agreement.SecondPartyToFirstPartyOneWayAgreement.SendPorts)
                                        if (!tpm.VerifySendPortExistsInOneWayAgreementAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, sendPort.Name, builder))
                                            tpm.AddSendPortToOneWayAgreementAgreement(party.Name, agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, sendPort.Name, builder);
                                }
                                if (agreement.Protocol == "X12" && agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings != null)
                                {
                                    if (agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings.Acknowledgement != null)
                                    {
                                        tpm.EnableFunctionalAckForX12Agreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings.Acknowledgement.Expect997, builder);
                                    }
                                    if (agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators != null)
                                    {
                                        tpm.SetCharacterSetAndSeparatorForX12Agreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name,
                                            agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators.DataElementSeparator + ":" +
                                        agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators.ComponentElementSeparator + ":" +
                                        agreement.SecondPartyToFirstPartyOneWayAgreement.InterchangeSettings.CharacterSetAndSeparators.SegmentTerminator, builder);
                                    }
                                   

                                }
                                if (agreement.Protocol == "X12" && agreement.SecondPartyToFirstPartyOneWayAgreement.TransactionSettings != null)
                                {
                                    if (agreement.SecondPartyToFirstPartyOneWayAgreement.TransactionSettings.Envelopes != null)
                                    {
                                        foreach (var envelope in agreement.SecondPartyToFirstPartyOneWayAgreement.TransactionSettings.Envelopes)
                                        {
                                            string messageid = "", protocolVersion = "", targetNamespace = "";
                                            messageid = envelope.TransactionType;
                                            protocolVersion = envelope.Version;
                                            targetNamespace = envelope.TargetNamespace;
                                            if (!tpm.VerifyEnvelopeExistsForX12OneWayAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, messageid + ":" + protocolVersion, targetNamespace,builder))
                                                tpm.AddEnvelopesForX12OneWayAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, messageid + ":" + protocolVersion, targetNamespace,
                                                    envelope.GS3, envelope.GS2, envelope.GS4 + ":" + envelope.GS5, envelope.GS7, envelope.GS1, builder);
                                        }
                                    }
                                    if (agreement.SecondPartyToFirstPartyOneWayAgreement.TransactionSettings.SchemaOverides != null && agreement.SecondPartyToFirstPartyOneWayAgreement.TransactionSettings.SchemaOverides.Count > 0)
                                    {
                                        foreach (var schemaOveride in agreement.SecondPartyToFirstPartyOneWayAgreement.TransactionSettings.SchemaOverides)
                                        {
                                            string messageid = "", protocolVersion = "", targetNamespace = "";
                                            messageid = schemaOveride.TransactionType;
                                            protocolVersion = schemaOveride.Version;
                                            targetNamespace = schemaOveride.TargetNamespace;
                                            if (!tpm.VerifySchemaOverideExistsForX12OneWayAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, messageid + ":" + protocolVersion, schemaOveride.GS2, builder))

                                                tpm.AddSchemaOveridesForX12OneWayAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, messageid + ":" + protocolVersion, targetNamespace,
                                                     schemaOveride.GS2, builder);
                                        }
                                    }
                                   
                                }
                                if (agreement.Protocol == "X12" && agreement.SecondPartyToFirstPartyOneWayAgreement.Batches != null && agreement.SecondPartyToFirstPartyOneWayAgreement.Batches.Count() > 0)
                                {
                                    foreach (var batch in agreement.SecondPartyToFirstPartyOneWayAgreement.Batches)
                                    {
                                        if (!tpm.VerifyBatchExistsForX12OneWayAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, batch.Name, builder))
                                        {
                                            tpm.AddBatchForX12OneWayAgreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, batch, builder);
                                        }
                                    }
                                }
                                if (agreement.Protocol == "AS2" && agreement.SecondPartyToFirstPartyOneWayAgreement.MDNAcknowledgementSettings != null)
                                {


                                    tpm.EnableMDNForAS2Agreement(agreement.Name, agreement.SecondParty.Name, agreement.FirstParty.Name, agreement.SecondPartyToFirstPartyOneWayAgreement.MDNAcknowledgementSettings.SigningAlgorithm, agreement.SecondPartyToFirstPartyOneWayAgreement.MDNAcknowledgementSettings.ReceiptDeliveryOptionUrl, builder);


                                }

                            }
                        }

                    }


                }

                Console.WriteLine("Completed");
                //Console.Read();

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
               // Console.Read();
            }
        }


    }
}
