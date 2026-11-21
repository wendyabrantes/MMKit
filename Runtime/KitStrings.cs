using System.Collections.Generic;
using UnityEngine;

namespace MightyMinds.Kit
{
    /// <summary>
    /// Built-in translations of the kit's own screens (About us, Settings, Help &amp; FAQ, grown-up gate).
    /// A translation is used only while the English text on screen still equals <see cref="English"/>: an app that
    /// passes its own wording (e.g. a custom AboutIntro), or English that changes later, falls back to English rather
    /// than showing a stale translation. Edit English and every language together.
    /// </summary>
    internal static class KitStrings
    {
        /// <summary>The kit languages, in the order Settings would list them.</summary>
        public static readonly string[] Languages = { "en", "es", "fr", "de", "it", "pt", "ru", "ja", "ko", "zh-Hans" };

        static string current;
        static string currentSource;

        /// <summary>The language in use: <see cref="MMKitConfig.Language"/> when set, else the device language.</summary>
        public static string Current
        {
            get
            {
                string wanted = MMKit.Config.Language;
                if (current != null && currentSource == wanted) return current;
                currentSource = wanted;
                current = !string.IsNullOrEmpty(wanted) ? wanted : FromDevice(Application.systemLanguage);
                return current;
            }
        }

        static string FromDevice(SystemLanguage lang)
        {
            switch (lang)
            {
                case SystemLanguage.Spanish:            return "es";
                case SystemLanguage.French:             return "fr";
                case SystemLanguage.German:             return "de";
                case SystemLanguage.Italian:            return "it";
                case SystemLanguage.Portuguese:         return "pt";
                case SystemLanguage.Russian:            return "ru";
                case SystemLanguage.Japanese:           return "ja";
                case SystemLanguage.Korean:             return "ko";
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:  return "zh-Hans";
                default:                                return "en";   // incl. Traditional Chinese until it has its own table
            }
        }

        /// <summary>The translation of <paramref name="key"/> in the current language, or null to show <paramref name="english"/>.</summary>
        public static string Get(string key, string english)
        {
            string lang = Current;
            if (lang == "en" || !Tables.TryGetValue(lang, out var table)) return null;
            if (!English.TryGetValue(key, out var source) || source != english) return null;
            return table.TryGetValue(key, out var s) ? s : null;
        }

        /// <summary>The English each translation was made from (must match the fallbacks in the kit's code).</summary>
        public static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "kit.gate.hint", "Solve the problem to continue…" },
            { "kit.review.gate_title", "Grown-ups: solve this to leave us a rating" },
            { "kit.nav.about", "About us" },
            { "kit.nav.settings", "Settings" },
            { "kit.nav.faq", "Help & FAQ" },
            { "kit.common.back", "Back" },
            { "kit.about.intro", "We build fun, thoughtful learning apps for the devices kids already love." },
            { "kit.about.description", "We’re two friends who believe learning should feel like play." },
            { "kit.about.trust", "Why parents trust us" },
            { "kit.about.rate_title", "Enjoying our apps?" },
            { "kit.about.rate_body", "A quick review helps other parents find us." },
            { "kit.about.terms", "Terms of use" },
            { "kit.about.privacy", "Privacy policy" },
            { "kit.about.feature1.title", "Built for little hands" },
            { "kit.about.feature1.desc", "Bright, simple screens designed for how small children really use touchscreens." },
            { "kit.about.feature2.title", "Real learning, real fun" },
            { "kit.about.feature2.desc", "Grounded in proven early-years principles, wrapped in colourful play." },
            { "kit.about.feature3.title", "Safe for kids" },
            { "kit.about.feature3.desc", "No ads. No hidden purchases. No worries — just content you can trust." },
            { "kit.settings.sound", "SOUND" },
            { "kit.settings.music", "Music" },
            { "kit.settings.privacy", "PRIVACY" },
            { "kit.settings.analytics", "Share anonymous usage stats" },
            { "kit.settings.support", "SUPPORT" },
            { "kit.settings.rate", "Rate us" },
            { "kit.settings.restore", "Restore purchases" },
            { "kit.settings.redeem", "Redeem code" },
            { "kit.settings.version", "Version" },
            { "kit.faq.contact", "Get in touch" },
            { "kit.faq.contact_body", "Questions or feedback? We read every message." },
            { "kit.faq.title", "Frequently asked questions" },
            { "kit.faq.more_title", "Still have questions?" },
            { "kit.faq.more_body", "Visit our website for more answers." },
            { "kit.faq.more_button", "More Q&As" },
            { "kit.faq.q1", "Is it a one-off payment or a subscription?" },
            { "kit.faq.a1", "Our apps are a one-time purchase — no subscriptions. Some games are free to try before you unlock the rest." },
            { "kit.faq.q2", "How do I restore my purchase?" },
            { "kit.faq.a2", "Open Settings and tap Restore purchases, then sign in with the same App Store or Google Play account you bought with." },
            { "kit.faq.q3", "Does it need an internet connection?" },
            { "kit.faq.a3", "No. Once the app is installed, the games work offline." },
            { "kit.faq.q4", "Are there any ads?" },
            { "kit.faq.a4", "No. There are no ads and no third-party tracking in our apps." },
        };

        static readonly Dictionary<string, Dictionary<string, string>> Tables = new Dictionary<string, Dictionary<string, string>>
        {
            // ── Spanish (tú) ──────────────────────────────────────────────────
            ["es"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "Resuelve el problema para continuar…" },
                { "kit.review.gate_title", "Adultos: resuelve esto para dejarnos una valoración" },
                { "kit.nav.about", "Sobre nosotros" },
                { "kit.nav.settings", "Ajustes" },
                { "kit.nav.faq", "Ayuda y preguntas" },
                { "kit.common.back", "Atrás" },
                { "kit.about.intro", "Creamos apps educativas divertidas y cuidadas para los dispositivos que a los niños ya les encantan." },
                { "kit.about.description", "Somos dos amigos que creen que aprender debería sentirse como jugar." },
                { "kit.about.trust", "Por qué confían en nosotros las familias" },
                { "kit.about.rate_title", "¿Te gustan nuestras apps?" },
                { "kit.about.rate_body", "Una reseña rápida ayuda a otras familias a encontrarnos." },
                { "kit.about.terms", "Condiciones de uso" },
                { "kit.about.privacy", "Política de privacidad" },
                { "kit.about.feature1.title", "Pensado para manos pequeñas" },
                { "kit.about.feature1.desc", "Pantallas sencillas y coloridas, diseñadas según cómo usan de verdad los más pequeños las pantallas táctiles." },
                { "kit.about.feature2.title", "Aprendizaje real, diversión real" },
                { "kit.about.feature2.desc", "Basado en principios probados de educación infantil y envuelto en juego lleno de color." },
                { "kit.about.feature3.title", "Seguro para niños" },
                { "kit.about.feature3.desc", "Sin anuncios. Sin compras ocultas. Sin preocupaciones: solo contenido de confianza." },
                { "kit.settings.sound", "SONIDO" },
                { "kit.settings.music", "Música" },
                { "kit.settings.privacy", "PRIVACIDAD" },
                { "kit.settings.analytics", "Compartir estadísticas de uso anónimas" },
                { "kit.settings.support", "SOPORTE" },
                { "kit.settings.rate", "Valóranos" },
                { "kit.settings.restore", "Restaurar compras" },
                { "kit.settings.redeem", "Canjear código" },
                { "kit.settings.version", "Versión" },
                { "kit.faq.contact", "Contacta con nosotros" },
                { "kit.faq.contact_body", "¿Preguntas o comentarios? Leemos todos los mensajes." },
                { "kit.faq.title", "Preguntas frecuentes" },
                { "kit.faq.more_title", "¿Tienes más preguntas?" },
                { "kit.faq.more_body", "Visita nuestra web para encontrar más respuestas." },
                { "kit.faq.more_button", "Más respuestas" },
                { "kit.faq.q1", "¿Es un pago único o una suscripción?" },
                { "kit.faq.a1", "Nuestras apps se compran una sola vez, sin suscripciones. Algunos juegos se pueden probar gratis antes de desbloquear el resto." },
                { "kit.faq.q2", "¿Cómo restauro mi compra?" },
                { "kit.faq.a2", "Abre Ajustes y toca Restaurar compras; después inicia sesión con la misma cuenta de App Store o Google Play con la que compraste." },
                { "kit.faq.q3", "¿Necesita conexión a internet?" },
                { "kit.faq.a3", "No. Una vez instalada la app, los juegos funcionan sin conexión." },
                { "kit.faq.q4", "¿Tiene anuncios?" },
                { "kit.faq.a4", "No. Nuestras apps no tienen anuncios ni rastreo de terceros." },
            },

            // ── French (vous; non-breaking space before ? ! :) ──────────────
            ["fr"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "Résolvez le problème pour continuer…" },
                { "kit.review.gate_title", "Pour les adultes : résolvez ceci pour nous laisser une note" },
                { "kit.nav.about", "À propos" },
                { "kit.nav.settings", "Réglages" },
                { "kit.nav.faq", "Aide et FAQ" },
                { "kit.common.back", "Retour" },
                { "kit.about.intro", "Nous créons des apps éducatives ludiques et soignées pour les appareils que les enfants adorent déjà." },
                { "kit.about.description", "Nous sommes deux amis convaincus qu’apprendre doit ressembler à un jeu." },
                { "kit.about.trust", "Pourquoi les parents nous font confiance" },
                { "kit.about.rate_title", "Vous aimez nos apps ?" },
                { "kit.about.rate_body", "Un petit avis aide d’autres parents à nous trouver." },
                { "kit.about.terms", "Conditions d’utilisation" },
                { "kit.about.privacy", "Politique de confidentialité" },
                { "kit.about.feature1.title", "Pensé pour les petites mains" },
                { "kit.about.feature1.desc", "Des écrans simples et colorés, conçus pour la façon dont les jeunes enfants utilisent vraiment les écrans tactiles." },
                { "kit.about.feature2.title", "Vrai apprentissage, vrai plaisir" },
                { "kit.about.feature2.desc", "Fondé sur des principes éprouvés de la petite enfance, dans un univers de jeu coloré." },
                { "kit.about.feature3.title", "Sans danger pour les enfants" },
                { "kit.about.feature3.desc", "Pas de publicité. Pas d’achats cachés. Pas de souci : seulement du contenu fiable." },
                { "kit.settings.sound", "SON" },
                { "kit.settings.music", "Musique" },
                { "kit.settings.privacy", "CONFIDENTIALITÉ" },
                { "kit.settings.analytics", "Partager des statistiques d’utilisation anonymes" },
                { "kit.settings.support", "ASSISTANCE" },
                { "kit.settings.rate", "Nous noter" },
                { "kit.settings.restore", "Restaurer les achats" },
                { "kit.settings.redeem", "Utiliser un code" },
                { "kit.settings.version", "Version" },
                { "kit.faq.contact", "Nous contacter" },
                { "kit.faq.contact_body", "Une question ou une remarque ? Nous lisons tous les messages." },
                { "kit.faq.title", "Questions fréquentes" },
                { "kit.faq.more_title", "Encore des questions ?" },
                { "kit.faq.more_body", "Rendez-vous sur notre site pour plus de réponses." },
                { "kit.faq.more_button", "Plus de réponses" },
                { "kit.faq.q1", "Est-ce un achat unique ou un abonnement ?" },
                { "kit.faq.a1", "Nos apps s’achètent une seule fois, sans abonnement. Certains jeux sont gratuits à l’essai avant de débloquer le reste." },
                { "kit.faq.q2", "Comment restaurer mon achat ?" },
                { "kit.faq.a2", "Ouvrez Réglages et touchez Restaurer les achats, puis connectez-vous avec le même compte App Store ou Google Play que lors de l’achat." },
                { "kit.faq.q3", "Faut-il une connexion internet ?" },
                { "kit.faq.a3", "Non. Une fois l’app installée, les jeux fonctionnent hors ligne." },
                { "kit.faq.q4", "Y a-t-il des publicités ?" },
                { "kit.faq.a4", "Non. Nos apps ne contiennent ni publicité ni suivi par des tiers." },
            },

            // ── German (Sie) ──────────────────────────────────────────────────
            ["de"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "Lösen Sie die Aufgabe, um fortzufahren…" },
                { "kit.review.gate_title", "Für Erwachsene: Lösen Sie die Aufgabe, um uns zu bewerten" },
                { "kit.nav.about", "Über uns" },
                { "kit.nav.settings", "Einstellungen" },
                { "kit.nav.faq", "Hilfe & FAQ" },
                { "kit.common.back", "Zurück" },
                { "kit.about.intro", "Wir entwickeln liebevolle Lern-Apps voller Spaß für die Geräte, die Kinder ohnehin lieben." },
                { "kit.about.description", "Wir sind zwei Freunde und finden: Lernen sollte sich wie Spielen anfühlen." },
                { "kit.about.trust", "Warum Eltern uns vertrauen" },
                { "kit.about.rate_title", "Gefallen Ihnen unsere Apps?" },
                { "kit.about.rate_body", "Eine kurze Bewertung hilft anderen Eltern, uns zu finden." },
                { "kit.about.terms", "Nutzungsbedingungen" },
                { "kit.about.privacy", "Datenschutzerklärung" },
                { "kit.about.feature1.title", "Für kleine Hände gemacht" },
                { "kit.about.feature1.desc", "Klare, farbenfrohe Bildschirme – so gestaltet, wie kleine Kinder Touchscreens wirklich nutzen." },
                { "kit.about.feature2.title", "Echtes Lernen, echter Spaß" },
                { "kit.about.feature2.desc", "Auf bewährten Prinzipien der frühkindlichen Bildung aufgebaut und in buntes Spiel verpackt." },
                { "kit.about.feature3.title", "Sicher für Kinder" },
                { "kit.about.feature3.desc", "Keine Werbung. Keine versteckten Käufe. Keine Sorgen – nur Inhalte, denen Sie vertrauen können." },
                { "kit.settings.sound", "TON" },
                { "kit.settings.music", "Musik" },
                { "kit.settings.privacy", "DATENSCHUTZ" },
                { "kit.settings.analytics", "Anonyme Nutzungsstatistiken teilen" },
                { "kit.settings.support", "HILFE" },
                { "kit.settings.rate", "Bewerten" },
                { "kit.settings.restore", "Käufe wiederherstellen" },
                { "kit.settings.redeem", "Code einlösen" },
                { "kit.settings.version", "Version" },
                { "kit.faq.contact", "Kontakt" },
                { "kit.faq.contact_body", "Fragen oder Feedback? Wir lesen jede Nachricht." },
                { "kit.faq.title", "Häufige Fragen" },
                { "kit.faq.more_title", "Noch Fragen?" },
                { "kit.faq.more_body", "Weitere Antworten finden Sie auf unserer Website." },
                { "kit.faq.more_button", "Mehr Antworten" },
                { "kit.faq.q1", "Ist es ein Einmalkauf oder ein Abo?" },
                { "kit.faq.a1", "Unsere Apps sind ein Einmalkauf – kein Abo. Einige Spiele können Sie kostenlos testen, bevor Sie den Rest freischalten." },
                { "kit.faq.q2", "Wie stelle ich meinen Kauf wieder her?" },
                { "kit.faq.a2", "Öffnen Sie die Einstellungen, tippen Sie auf „Käufe wiederherstellen“ und melden Sie sich mit demselben App-Store- oder Google-Play-Konto an, mit dem Sie gekauft haben." },
                { "kit.faq.q3", "Braucht die App eine Internetverbindung?" },
                { "kit.faq.a3", "Nein. Sobald die App installiert ist, funktionieren die Spiele offline." },
                { "kit.faq.q4", "Gibt es Werbung?" },
                { "kit.faq.a4", "Nein. Unsere Apps enthalten keine Werbung und kein Tracking durch Dritte." },
            },

            // ── Italian (tu) ──────────────────────────────────────────────────
            ["it"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "Risolvi il problema per continuare…" },
                { "kit.review.gate_title", "Per gli adulti: risolvi questo per lasciarci una recensione" },
                { "kit.nav.about", "Chi siamo" },
                { "kit.nav.settings", "Impostazioni" },
                { "kit.nav.faq", "Aiuto e FAQ" },
                { "kit.common.back", "Indietro" },
                { "kit.about.intro", "Creiamo app educative divertenti e curate per i dispositivi che i bambini amano già." },
                { "kit.about.description", "Siamo due amici convinti che imparare debba sembrare un gioco." },
                { "kit.about.trust", "Perché i genitori si fidano di noi" },
                { "kit.about.rate_title", "Ti piacciono le nostre app?" },
                { "kit.about.rate_body", "Una breve recensione aiuta altri genitori a trovarci." },
                { "kit.about.terms", "Termini d’uso" },
                { "kit.about.privacy", "Informativa sulla privacy" },
                { "kit.about.feature1.title", "Pensata per le piccole mani" },
                { "kit.about.feature1.desc", "Schermate semplici e colorate, progettate per come i più piccoli usano davvero i touchscreen." },
                { "kit.about.feature2.title", "Vero apprendimento, vero divertimento" },
                { "kit.about.feature2.desc", "Basata su principi collaudati per la prima infanzia, avvolti in un gioco pieno di colori." },
                { "kit.about.feature3.title", "Sicura per i bambini" },
                { "kit.about.feature3.desc", "Niente pubblicità. Niente acquisti nascosti. Nessuna preoccupazione: solo contenuti affidabili." },
                { "kit.settings.sound", "AUDIO" },
                { "kit.settings.music", "Musica" },
                { "kit.settings.privacy", "PRIVACY" },
                { "kit.settings.analytics", "Condividi statistiche d’uso anonime" },
                { "kit.settings.support", "ASSISTENZA" },
                { "kit.settings.rate", "Valutaci" },
                { "kit.settings.restore", "Ripristina acquisti" },
                { "kit.settings.redeem", "Riscatta codice" },
                { "kit.settings.version", "Versione" },
                { "kit.faq.contact", "Contattaci" },
                { "kit.faq.contact_body", "Domande o suggerimenti? Leggiamo ogni messaggio." },
                { "kit.faq.title", "Domande frequenti" },
                { "kit.faq.more_title", "Hai altre domande?" },
                { "kit.faq.more_body", "Visita il nostro sito per altre risposte." },
                { "kit.faq.more_button", "Altre risposte" },
                { "kit.faq.q1", "È un pagamento unico o un abbonamento?" },
                { "kit.faq.a1", "Le nostre app si acquistano una sola volta, senza abbonamenti. Alcuni giochi si possono provare gratis prima di sbloccare il resto." },
                { "kit.faq.q2", "Come ripristino il mio acquisto?" },
                { "kit.faq.a2", "Apri Impostazioni e tocca Ripristina acquisti, poi accedi con lo stesso account App Store o Google Play usato per l’acquisto." },
                { "kit.faq.q3", "Serve una connessione a internet?" },
                { "kit.faq.a3", "No. Una volta installata l’app, i giochi funzionano offline." },
                { "kit.faq.q4", "Ci sono pubblicità?" },
                { "kit.faq.a4", "No. Nelle nostre app non ci sono pubblicità né tracciamento di terze parti." },
            },

            // ── Portuguese (Brazil, você) ─────────────────────────────────────
            ["pt"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "Resolva o problema para continuar…" },
                { "kit.review.gate_title", "Para adultos: resolva isto para nos deixar uma avaliação" },
                { "kit.nav.about", "Sobre nós" },
                { "kit.nav.settings", "Ajustes" },
                { "kit.nav.faq", "Ajuda e dúvidas" },
                { "kit.common.back", "Voltar" },
                { "kit.about.intro", "Criamos apps educativos divertidos e caprichados para os aparelhos que as crianças já adoram." },
                { "kit.about.description", "Somos dois amigos que acreditam que aprender deve ser como brincar." },
                { "kit.about.trust", "Por que os pais confiam em nós" },
                { "kit.about.rate_title", "Está gostando dos nossos apps?" },
                { "kit.about.rate_body", "Uma avaliação rápida ajuda outros pais a nos encontrar." },
                { "kit.about.terms", "Termos de uso" },
                { "kit.about.privacy", "Política de privacidade" },
                { "kit.about.feature1.title", "Feito para mãozinhas" },
                { "kit.about.feature1.desc", "Telas simples e coloridas, pensadas para o jeito como as crianças pequenas realmente usam telas sensíveis ao toque." },
                { "kit.about.feature2.title", "Aprendizado de verdade, diversão de verdade" },
                { "kit.about.feature2.desc", "Baseado em princípios comprovados da educação infantil, com muita brincadeira colorida." },
                { "kit.about.feature3.title", "Seguro para crianças" },
                { "kit.about.feature3.desc", "Sem anúncios. Sem compras escondidas. Sem preocupações: só conteúdo confiável." },
                { "kit.settings.sound", "SOM" },
                { "kit.settings.music", "Música" },
                { "kit.settings.privacy", "PRIVACIDADE" },
                { "kit.settings.analytics", "Compartilhar estatísticas de uso anônimas" },
                { "kit.settings.support", "SUPORTE" },
                { "kit.settings.rate", "Avalie-nos" },
                { "kit.settings.restore", "Restaurar compras" },
                { "kit.settings.redeem", "Resgatar código" },
                { "kit.settings.version", "Versão" },
                { "kit.faq.contact", "Fale conosco" },
                { "kit.faq.contact_body", "Dúvidas ou sugestões? Lemos todas as mensagens." },
                { "kit.faq.title", "Perguntas frequentes" },
                { "kit.faq.more_title", "Ainda tem dúvidas?" },
                { "kit.faq.more_body", "Visite nosso site para mais respostas." },
                { "kit.faq.more_button", "Mais respostas" },
                { "kit.faq.q1", "É pagamento único ou assinatura?" },
                { "kit.faq.a1", "Nossos apps são uma compra única, sem assinaturas. Alguns jogos são gratuitos para testar antes de desbloquear o resto." },
                { "kit.faq.q2", "Como restauro minha compra?" },
                { "kit.faq.a2", "Abra Ajustes e toque em Restaurar compras; depois entre com a mesma conta da App Store ou do Google Play usada na compra." },
                { "kit.faq.q3", "Precisa de internet?" },
                { "kit.faq.a3", "Não. Depois que o app é instalado, os jogos funcionam offline." },
                { "kit.faq.q4", "Tem anúncios?" },
                { "kit.faq.a4", "Não. Nossos apps não têm anúncios nem rastreamento de terceiros." },
            },

            // ── Russian (вы) ──────────────────────────────────────────────────
            ["ru"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "Решите пример, чтобы продолжить…" },
                { "kit.review.gate_title", "Для взрослых: решите пример, чтобы оставить оценку" },
                { "kit.nav.about", "О нас" },
                { "kit.nav.settings", "Настройки" },
                { "kit.nav.faq", "Помощь и вопросы" },
                { "kit.common.back", "Назад" },
                { "kit.about.intro", "Мы создаём весёлые и продуманные обучающие приложения для устройств, которые дети уже любят." },
                { "kit.about.description", "Мы — двое друзей и верим, что учиться должно быть так же интересно, как играть." },
                { "kit.about.trust", "Почему родители нам доверяют" },
                { "kit.about.rate_title", "Нравятся наши приложения?" },
                { "kit.about.rate_body", "Короткий отзыв поможет другим родителям найти нас." },
                { "kit.about.terms", "Условия использования" },
                { "kit.about.privacy", "Политика конфиденциальности" },
                { "kit.about.feature1.title", "Для маленьких ручек" },
                { "kit.about.feature1.desc", "Яркие и простые экраны, созданные с учётом того, как малыши на самом деле пользуются сенсорными экранами." },
                { "kit.about.feature2.title", "Настоящее обучение, настоящее веселье" },
                { "kit.about.feature2.desc", "В основе — проверенные принципы раннего развития, а в оформлении — яркая игра." },
                { "kit.about.feature3.title", "Безопасно для детей" },
                { "kit.about.feature3.desc", "Никакой рекламы. Никаких скрытых покупок. Никаких забот — только надёжный контент." },
                { "kit.settings.sound", "ЗВУК" },
                { "kit.settings.music", "Музыка" },
                { "kit.settings.privacy", "КОНФИДЕНЦИАЛЬНОСТЬ" },
                { "kit.settings.analytics", "Отправлять анонимную статистику" },
                { "kit.settings.support", "ПОДДЕРЖКА" },
                { "kit.settings.rate", "Оценить" },
                { "kit.settings.restore", "Восстановить покупки" },
                { "kit.settings.redeem", "Активировать код" },
                { "kit.settings.version", "Версия" },
                { "kit.faq.contact", "Напишите нам" },
                { "kit.faq.contact_body", "Вопросы или отзывы? Мы читаем каждое сообщение." },
                { "kit.faq.title", "Частые вопросы" },
                { "kit.faq.more_title", "Остались вопросы?" },
                { "kit.faq.more_body", "Больше ответов — на нашем сайте." },
                { "kit.faq.more_button", "Больше ответов" },
                { "kit.faq.q1", "Это разовая покупка или подписка?" },
                { "kit.faq.a1", "Наши приложения покупаются один раз — без подписок. Некоторые игры можно попробовать бесплатно, прежде чем открыть остальные." },
                { "kit.faq.q2", "Как восстановить покупку?" },
                { "kit.faq.a2", "Откройте «Настройки» и нажмите «Восстановить покупки», затем войдите в тот же аккаунт App Store или Google Play, с которого совершали покупку." },
                { "kit.faq.q3", "Нужен ли интернет?" },
                { "kit.faq.a3", "Нет. После установки приложения игры работают без интернета." },
                { "kit.faq.q4", "Есть ли реклама?" },
                { "kit.faq.a4", "Нет. В наших приложениях нет рекламы и стороннего отслеживания." },
            },

            // ── Japanese ──────────────────────────────────────────────────────
            ["ja"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "問題を解いて進んでください…" },
                { "kit.review.gate_title", "保護者の方へ：評価するにはこの問題を解いてください" },
                { "kit.nav.about", "私たちについて" },
                { "kit.nav.settings", "設定" },
                { "kit.nav.faq", "ヘルプ・質問" },
                { "kit.common.back", "戻る" },
                { "kit.about.intro", "子どもたちが大好きなデバイスで遊べる、楽しくて心のこもった学習アプリをつくっています。" },
                { "kit.about.description", "「学びは遊びのように楽しいものであるべき」と考える、ふたりの友人です。" },
                { "kit.about.trust", "保護者の方に選ばれる理由" },
                { "kit.about.rate_title", "アプリを楽しんでいますか？" },
                { "kit.about.rate_body", "レビューをいただけると、ほかの保護者の方に見つけてもらいやすくなります。" },
                { "kit.about.terms", "利用規約" },
                { "kit.about.privacy", "プライバシーポリシー" },
                { "kit.about.feature1.title", "小さな手のために" },
                { "kit.about.feature1.desc", "小さな子どもが実際にタッチスクリーンを使う様子に合わせた、明るくシンプルな画面。" },
                { "kit.about.feature2.title", "本物の学びと楽しさ" },
                { "kit.about.feature2.desc", "実証された幼児教育の考え方を、カラフルな遊びで包みました。" },
                { "kit.about.feature3.title", "子どもも安心" },
                { "kit.about.feature3.desc", "広告なし。隠れた課金なし。安心して使えるコンテンツだけ。" },
                { "kit.settings.sound", "サウンド" },
                { "kit.settings.music", "音楽" },
                { "kit.settings.privacy", "プライバシー" },
                { "kit.settings.analytics", "匿名の利用統計を送信" },
                { "kit.settings.support", "サポート" },
                { "kit.settings.rate", "評価する" },
                { "kit.settings.restore", "購入を復元" },
                { "kit.settings.redeem", "コードを使う" },
                { "kit.settings.version", "バージョン" },
                { "kit.faq.contact", "お問い合わせ" },
                { "kit.faq.contact_body", "ご質問やご意見をお寄せください。すべてのメッセージに目を通しています。" },
                { "kit.faq.title", "よくある質問" },
                { "kit.faq.more_title", "ほかにご質問はありますか？" },
                { "kit.faq.more_body", "ウェブサイトでさらに詳しくご案内しています。" },
                { "kit.faq.more_button", "もっと見る" },
                { "kit.faq.q1", "買い切りですか？サブスクリプションですか？" },
                { "kit.faq.a1", "買い切りで、サブスクリプションはありません。一部のゲームは無料で試してから、残りをアンロックできます。" },
                { "kit.faq.q2", "購入を復元するには？" },
                { "kit.faq.a2", "設定を開いて「購入を復元」をタップし、購入時と同じ App Store または Google Play のアカウントでサインインしてください。" },
                { "kit.faq.q3", "インターネット接続は必要ですか？" },
                { "kit.faq.a3", "いいえ。アプリをインストールすれば、ゲームはオフラインで遊べます。" },
                { "kit.faq.q4", "広告はありますか？" },
                { "kit.faq.a4", "いいえ。広告も第三者によるトラッキングもありません。" },
            },

            // ── Korean ────────────────────────────────────────────────────────
            ["ko"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "문제를 풀면 계속할 수 있어요…" },
                { "kit.review.gate_title", "보호자분께: 평가를 남기려면 이 문제를 풀어 주세요" },
                { "kit.nav.about", "소개" },
                { "kit.nav.settings", "설정" },
                { "kit.nav.faq", "도움말 및 FAQ" },
                { "kit.common.back", "뒤로" },
                { "kit.about.intro", "아이들이 이미 좋아하는 기기에서 즐길 수 있는, 재미있고 세심한 학습 앱을 만듭니다." },
                { "kit.about.description", "배움은 놀이처럼 즐거워야 한다고 믿는 두 친구입니다." },
                { "kit.about.trust", "부모님이 믿고 선택하는 이유" },
                { "kit.about.rate_title", "저희 앱이 마음에 드시나요?" },
                { "kit.about.rate_body", "짧은 리뷰가 다른 부모님들이 저희를 찾는 데 큰 도움이 돼요." },
                { "kit.about.terms", "이용약관" },
                { "kit.about.privacy", "개인정보 처리방침" },
                { "kit.about.feature1.title", "작은 손을 위해" },
                { "kit.about.feature1.desc", "어린아이들이 실제로 터치스크린을 사용하는 방식에 맞춘 밝고 단순한 화면." },
                { "kit.about.feature2.title", "진짜 배움, 진짜 재미" },
                { "kit.about.feature2.desc", "검증된 유아 교육 원칙을 알록달록한 놀이에 담았어요." },
                { "kit.about.feature3.title", "아이에게 안전하게" },
                { "kit.about.feature3.desc", "광고 없음. 숨은 결제 없음. 걱정 없이 믿을 수 있는 콘텐츠만." },
                { "kit.settings.sound", "사운드" },
                { "kit.settings.music", "음악" },
                { "kit.settings.privacy", "개인정보" },
                { "kit.settings.analytics", "익명 사용 통계 공유" },
                { "kit.settings.support", "지원" },
                { "kit.settings.rate", "평가하기" },
                { "kit.settings.restore", "구매 복원" },
                { "kit.settings.redeem", "코드 사용" },
                { "kit.settings.version", "버전" },
                { "kit.faq.contact", "문의하기" },
                { "kit.faq.contact_body", "질문이나 의견이 있으신가요? 모든 메시지를 꼼꼼히 읽고 있어요." },
                { "kit.faq.title", "자주 묻는 질문" },
                { "kit.faq.more_title", "더 궁금한 점이 있으신가요?" },
                { "kit.faq.more_body", "웹사이트에서 더 많은 답변을 확인하세요." },
                { "kit.faq.more_button", "더 많은 답변" },
                { "kit.faq.q1", "한 번 구매인가요, 구독인가요?" },
                { "kit.faq.a1", "저희 앱은 한 번만 구매하면 되고 구독은 없어요. 일부 게임은 무료로 먼저 체험한 뒤 나머지를 잠금 해제할 수 있어요." },
                { "kit.faq.q2", "구매를 어떻게 복원하나요?" },
                { "kit.faq.a2", "설정을 열고 구매 복원을 탭한 다음, 구매할 때 사용한 App Store 또는 Google Play 계정으로 로그인하세요." },
                { "kit.faq.q3", "인터넷 연결이 필요한가요?" },
                { "kit.faq.a3", "아니요. 앱을 설치하면 게임은 오프라인에서도 작동해요." },
                { "kit.faq.q4", "광고가 있나요?" },
                { "kit.faq.a4", "아니요. 저희 앱에는 광고도, 제3자 추적도 없어요." },
            },

            // ── Chinese (Simplified) ──────────────────────────────────────────
            ["zh-Hans"] = new Dictionary<string, string>
            {
                { "kit.gate.hint", "解开这道题即可继续…" },
                { "kit.review.gate_title", "家长请注意：解开这道题即可为我们评分" },
                { "kit.nav.about", "关于我们" },
                { "kit.nav.settings", "设置" },
                { "kit.nav.faq", "帮助与常见问题" },
                { "kit.common.back", "返回" },
                { "kit.about.intro", "我们为孩子们早已喜爱的设备，打造有趣又用心的学习应用。" },
                { "kit.about.description", "我们是两个好朋友，相信学习就应该像玩耍一样快乐。" },
                { "kit.about.trust", "家长信任我们的理由" },
                { "kit.about.rate_title", "喜欢我们的应用吗？" },
                { "kit.about.rate_body", "一条简短的评价，就能帮助更多家长找到我们。" },
                { "kit.about.terms", "使用条款" },
                { "kit.about.privacy", "隐私政策" },
                { "kit.about.feature1.title", "为小小的手设计" },
                { "kit.about.feature1.desc", "明亮简洁的界面，贴合小朋友真实使用触摸屏的方式。" },
                { "kit.about.feature2.title", "真正学习，真正有趣" },
                { "kit.about.feature2.desc", "基于经过验证的幼儿教育理念，融入多彩的游戏之中。" },
                { "kit.about.feature3.title", "对孩子安全" },
                { "kit.about.feature3.desc", "没有广告。没有隐藏消费。无需担心，只有值得信赖的内容。" },
                { "kit.settings.sound", "声音" },
                { "kit.settings.music", "音乐" },
                { "kit.settings.privacy", "隐私" },
                { "kit.settings.analytics", "分享匿名使用统计" },
                { "kit.settings.support", "支持" },
                { "kit.settings.rate", "给我们评分" },
                { "kit.settings.restore", "恢复购买" },
                { "kit.settings.redeem", "兑换代码" },
                { "kit.settings.version", "版本" },
                { "kit.faq.contact", "联系我们" },
                { "kit.faq.contact_body", "有问题或建议？每条消息我们都会认真阅读。" },
                { "kit.faq.title", "常见问题" },
                { "kit.faq.more_title", "还有疑问？" },
                { "kit.faq.more_body", "访问我们的网站，获取更多解答。" },
                { "kit.faq.more_button", "更多问答" },
                { "kit.faq.q1", "是一次性付费还是订阅？" },
                { "kit.faq.a1", "我们的应用是一次性购买，没有订阅。部分游戏可以先免费试玩，再解锁其余内容。" },
                { "kit.faq.q2", "如何恢复购买？" },
                { "kit.faq.a2", "打开“设置”并点按“恢复购买”，然后登录购买时使用的 App Store 或 Google Play 账户。" },
                { "kit.faq.q3", "需要联网吗？" },
                { "kit.faq.a3", "不需要。应用安装后，游戏可离线运行。" },
                { "kit.faq.q4", "有广告吗？" },
                { "kit.faq.a4", "没有。我们的应用没有广告，也没有第三方追踪。" },
            },
        };

        /// <summary>Every string in one language (used by the font subsetting tool and tests).</summary>
        internal static IEnumerable<string> All(string lang)
        {
            if (Tables.TryGetValue(lang, out var t)) return t.Values;
            return English.Values;
        }
    }
}
