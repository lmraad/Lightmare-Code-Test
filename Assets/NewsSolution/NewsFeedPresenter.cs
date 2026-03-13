using System;
using System.Threading.Tasks;
using InfinityHeroes.News.Framework;
using InfinityHeroes.News.Steam;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace InfinityHeroes.News.UI
{
    public sealed class NewsFeedPresenter : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] Button m_LoadArticlesButton;
        [SerializeField] NewsArticleCardView[] m_ArticleViews = Array.Empty<NewsArticleCardView>();
        [SerializeField] TMP_Text m_StatusText;

        [Header("Behaviour")]
        [SerializeField] bool m_LoadOnStart;

        bool m_IsLoading;

        void Awake()
        {
            if (m_LoadArticlesButton != null)
            {
                m_LoadArticlesButton.onClick.AddListener(LoadArticles);
            }

            ClearArticleViews();
            SetStatus("Press the button to load the latest articles.");
        }

        async void Start()
        {
            if (m_LoadOnStart)
            {
                await LoadArticlesAsync();
            }
        }

        void OnDestroy()
        {
            if (m_LoadArticlesButton != null)
            {
                m_LoadArticlesButton.onClick.RemoveListener(LoadArticles);
            }
        }

        public async void LoadArticles()
        {
            await LoadArticlesAsync();
        }

        async Task LoadArticlesAsync()
        {
            if (m_IsLoading)
            {
                return;
            }

            m_IsLoading = true;
            SetInteractable(false);
            SetStatus("Loading articles...");
            PrepareArticleViewsForRefresh();

            try
            {
                NewsClient newsClient = new();
                INewsResponse response = await newsClient.GetArticlesAsync();
                INewsArticle[] articles = response?.Articles ?? Array.Empty<INewsArticle>();

                if (articles.Length == 0)
                {
                    ClearArticleViews();
                    SetStatus(GetErrorMessage(response) ?? "No articles were returned.");
                    return;
                }

                for (int i = 0; i < m_ArticleViews.Length; i++)
                {
                    NewsArticleCardView articleView = m_ArticleViews[i];
                    if (articleView == null)
                    {
                        continue;
                    }

                    if (i < articles.Length)
                    {
                        articleView.Bind(articles[i]);
                        articleView.gameObject.SetActive(true);
                    }
                    else
                    {
                        articleView.Clear();
                        articleView.gameObject.SetActive(false);
                    }
                }

                SetStatus($"Loaded {Math.Min(articles.Length, m_ArticleViews.Length)} article(s).");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
                ClearArticleViews();
                SetStatus($"Failed to load articles: {exception.Message}");
            }
            finally
            {
                m_IsLoading = false;
                SetInteractable(true);
            }
        }

        void PrepareArticleViewsForRefresh()
        {
            foreach (NewsArticleCardView articleView in m_ArticleViews)
            {
                if (articleView == null)
                {
                    continue;
                }

                articleView.gameObject.SetActive(true);
                articleView.PrepareForRefresh();
            }
        }

        void ClearArticleViews()
        {
            foreach (NewsArticleCardView articleView in m_ArticleViews)
            {
                if (articleView == null)
                {
                    continue;
                }

                articleView.Clear();
                articleView.gameObject.SetActive(true);
            }
        }

        void SetInteractable(bool isInteractable)
        {
            if (m_LoadArticlesButton != null)
            {
                m_LoadArticlesButton.interactable = isInteractable;
            }
        }

        void SetStatus(string message)
        {
            if (m_StatusText != null)
            {
                m_StatusText.text = message;
            }
        }

        static string GetErrorMessage(INewsResponse response)
        {
            if (response is SteamNewsResponse steamResponse && steamResponse.IsError)
            {
                return $"Failed to load articles: {steamResponse.ErrorMessage}";
            }

            return null;
        }
    }
}
