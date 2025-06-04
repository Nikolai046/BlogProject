using BlogProject.Core.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace BlogProject.Data.Methods;

public interface IMethods
{
    /// <summary>
    /// Получает список всех статей всех пользователей с пагинацией.
    /// Возвращает список ArticleViewModel и флаг hasMore.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// /// </summary>
    Task<(List<ArticleViewModel>, bool)> GetAllArticles(int page, int pageSize = 10);

    /// <summary>
    /// Получает список всех статей всех пользователей с фильтрацией по тегам с пагинацией.
    /// Возвращает список ArticleViewModel и флаг hasMore.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// /// </summary>
    Task<(List<ArticleViewModel>, bool)> GetAllArticlesByTag(List<string> tags, int page, int pageSize = 10);

    /// <summary>
    /// Получает список статей конкретного пользователя с пагинацией.
    /// Возвращает список ArticleViewModel и флаг hasMore.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// /// </summary>
    Task<(List<ArticleViewModel>, bool)> GetArticlesByUserId(string? userId, int page, int pageSize = 10);

    /// <summary>
    /// Получает статью по указанному идентификатору.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// </summary>
    /// <param name="articleId">Идентификатор статьи, которую необходимо получить.</param>
    /// <returns>Возвращает объект ArticleViewModel, если статья найдена; в противном случае - null.</returns>
    Task<ArticleViewModel?> GetArticleById(int articleId);

    /// <summary>
    /// Создаёт новую статью с указанным заголовком, содержимым и тегами.
    /// </summary>
    Task CreateArticle(ArticleViewModel model);

    /// <summary>
    /// Редактирует статью по её ID. Доступ зависит от роли.
    /// </summary>
    Task EditArticle(int articleId, ArticleViewModel model);

    /// <summary>
    /// Удаляет статью по её ID. Доступ зависит от роли.
    /// </summary>
    Task DeleteArticle(int articleId);

    /// <summary>
    /// Добавляет комментарий к статье по её ID.
    /// </summary>
    Task CreateComment(int articleId, CommentViewModel model);

    /// <summary>
    /// Удаляет комментарий по его ID. Доступ зависит от роли.
    /// </summary>
    Task EditComment(int commentId, CommentViewModel model);

    /// <summary>
    /// Редактирует профиль пользователя по его ID. Доступ зависит от роли.
    /// </summary>
    Task<IdentityResult> EditUserProfile(string userId, UpdateUserViewModel profile);

    /// <summary>
    /// Удаляет пользователя по его ID. Доступ зависит от роли.
    /// </summary>
    Task DeleteUser(string userId);

    /// <summary>
    /// Возвращает список пользователей и флаг hasMore.
    /// Флаг Deletable зависит от роли и владения.
    /// </summary>
    /// <returns>Список пользователей.</returns>
    Task<(List<UserViewModel>, bool)> GetAllUsersAsync(int page, int pageSize = 10);

    Task<UserViewModel> GetUserInfoAsync(string? userId = null);
}

/*
 Пользователь
   - Получать все статьи.
   - Получить все статьи конкретного пользователя.
   - Получить статью по её ID.
   - Получить все статьи по тегу
   - Получать список всех пользователей.

   - Статьи: Может добавлять, редактировать (только свои), удалять (только свои).
   - Комментарии: Может добавлять, редактировать (только свои), удалять (только свои).
   - Профиль: Может редактировать свой профиль и удалить себя.

   
Администратор
   - Получать все статьи.
   - Получить все статьи конкретного пользователя.
   - Получить статью по её ID.
   - Получить все статьи по тегу
   - Получать список всех пользователей.

   - Статьи: Полный доступ (добавление, редактирование, удаление любых статей).
   - Комментарии: Полный доступ (добавление, редактирование, удаление любых комментариев).
   - Профиль: Может редактировать и удалять профили любых пользователей.

   
Модератор
   - Получать все статьи.
   - Получить все статьи конкретного пользователя.
   - Получить статью по её ID.
   - Получить все статьи по тегу
   - Получать список всех пользователей.

   - Статьи: Может редактировать любые статьи (без удаления).
   - Комментарии: Может редактировать любые комментарии (без удаления).  
   - Профиль: Может редактировать свой профиль и удалить себя.


*/