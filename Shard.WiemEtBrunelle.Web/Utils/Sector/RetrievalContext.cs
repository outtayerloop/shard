namespace Shard.WiemEtBrunelle.Web.Models
{
    /// <summary>
    /// Contexte d'une réponse HTTP renvoyant une ou plusieurs ressources
    /// </summary>
    public enum RetrievalContext
    {
        /// <summary>
        /// Récupération d'une seule ressource spécifique
        /// </summary>
        Single = 0,

        /// <summary>
        /// Récupération de plusieurs ressources
        /// </summary>
        Several = 1
    }
}
