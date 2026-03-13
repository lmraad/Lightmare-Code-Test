using System.Threading.Tasks;
using InfinityHeroes.News.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityHeroes.News.UI
{
    public sealed class NewsArticleCardView : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] TMP_Text m_TitleText;
        [SerializeField] TMP_Text m_ContentsText;
        [SerializeField] TMP_Text m_DateText;
        [SerializeField] RawImage m_Image;

        [Header("Interaction")]
        [SerializeField] Button m_OpenButton;

        [Header("State")]
        [SerializeField] GameObject m_LoadingIndicator;
        [SerializeField] Texture2D m_PlaceholderTexture;

        INewsArticle m_Article;
        int m_BindVersion;

        void Awake()
        {
            if (m_OpenButton != null)
            {
                m_OpenButton.onClick.AddListener(OpenArticle);
            }
        }

        void OnDestroy()
        {
            if (m_OpenButton != null)
            {
                m_OpenButton.onClick.RemoveListener(OpenArticle);
            }
        }

        public void Clear()
        {
            m_Article = null;
            m_BindVersion++;

            SetText(m_TitleText, "No article loaded");
            SetText(m_ContentsText, string.Empty);
            SetText(m_DateText, string.Empty);

            if (m_Image != null)
            {
                m_Image.texture = m_PlaceholderTexture;
                m_Image.enabled = m_PlaceholderTexture != null;
            }

            if (m_OpenButton != null)
            {
                m_OpenButton.interactable = false;
            }

            SetLoading(false);
        }

        public void Bind(INewsArticle article)
        {
            m_Article = article;
            int bindVersion = ++m_BindVersion;

            if (article == null)
            {
                Clear();
                return;
            }

            SetText(m_TitleText, article.Title);
            SetText(m_ContentsText, article.Contents);
            SetText(m_DateText, article.Date.ToLocalTime().ToString("dd MMM yyyy"));

            if (m_Image != null)
            {
                m_Image.texture = m_PlaceholderTexture;
                m_Image.enabled = true;
            }

            if (m_OpenButton != null)
            {
                m_OpenButton.interactable = true;
            }

            _ = LoadImageAsync(article, bindVersion);
        }

        public void PrepareForRefresh()
        {
            m_Article = null;
            m_BindVersion++;

            if (m_Image != null)
            {
                m_Image.texture = m_PlaceholderTexture;
                m_Image.enabled = m_PlaceholderTexture != null;
            }

            if (m_OpenButton != null)
            {
                m_OpenButton.interactable = false;
            }

            SetText(m_TitleText, "Refreshing...");
            SetText(m_ContentsText, string.Empty);
            SetText(m_DateText, string.Empty);
            SetLoading(true);
        }

        async Task LoadImageAsync(INewsArticle article, int bindVersion)
        {
            SetLoading(true);

            try
            {
                Texture2D texture = await article.GetTextureAsync();

                if (bindVersion != m_BindVersion || m_Article != article || m_Image == null)
                {
                    return;
                }

                if (texture != null)
                {
                    m_Image.texture = texture;
                    m_Image.enabled = true;
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                if (bindVersion == m_BindVersion)
                {
                    SetLoading(false);
                }
            }
        }

        void OpenArticle()
        {
            if (m_Article?.Source == null)
            {
                Debug.LogWarning("Article source was unavailable.", this);
                return;
            }

            m_Article.Source.Open();
        }

        void SetLoading(bool isLoading)
        {
            if (m_LoadingIndicator != null)
            {
                m_LoadingIndicator.SetActive(isLoading);
            }
        }

        static void SetText(TMP_Text label, string value)
        {
            if (label != null)
            {
                label.text = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
            }
        }
    }
}
