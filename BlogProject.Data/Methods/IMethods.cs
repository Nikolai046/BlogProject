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
    Task<(List<ArticleViewModel>, bool)> GetAllArticlesAsync(int page, int pageSize = 10);

    /// <summary>
    /// Получает список всех статей всех пользователей с фильтрацией по тегам с пагинацией.
    /// Возвращает список ArticleViewModel и флаг hasMore.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// /// </summary>
    Task<(List<ArticleViewModel>, bool)> GetAllArticlesByTagAsync(List<string> tags, int page, int pageSize = 10);

    /// <summary>
    /// Получает список статей конкретного пользователя с пагинацией.
    /// Возвращает список ArticleViewModel и флаг hasMore.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// /// </summary>
    Task<(List<ArticleViewModel>, bool)> GetArticlesByUserIdAsync(string? userId, int page, int pageSize = 10);

    /// <summary>
    /// Получает статью по указанному идентификатору.
    /// Флаг Deletable и Editable зависит от роли и владения.
    /// </summary>
    /// <param name="articleId">Идентификатор статьи, которую необходимо получить.</param>
    /// <returns>Возвращает объект ArticleViewModel, если статья найдена; в противном случае - null.</returns>
    Task<ArticleViewModel?> GetArticleByIdAsync(int articleId);

    /// <summary>
    /// Создаёт новую статью с указанным заголовком, содержимым и тегами.
    /// </summary>
    Task CreateArticleAsync(ArticleViewModel model);

    /// <summary>
    /// Редактирует статью по её ID. Доступ зависит от роли.
    /// </summary>
    Task EditArticleAsync(int articleId, ArticleViewModel model);

    /// <summary>
    /// Удаляет статью по её ID. Доступ зависит от роли.
    /// </summary>
    Task DeleteArticleAsync(int articleId);

    /// <summary>
    /// Добавляет комментарий к статье по её ID.
    /// </summary>
    Task CreateCommentAsync(int articleId, CommentViewModel model);

    /// <summary>
    /// Удаляет комментарий по его ID. Доступ зависит от роли.
    /// </summary>
    Task EditCommentAsync(int commentId, CommentViewModel model);

    /// <summary>
    /// Удаляет комментарий по его ID. Доступ зависит от роли.
    /// </summary>
    Task DeleteCommentAsync(int commentId);

    /// <summary>
    /// Редактирует профиль пользователя по его ID. Доступ зависит от роли.
    /// </summary>
    Task<IdentityResult> EditUserProfileAsync(UpdateUserViewModel profile, bool isAdminEditingOtherUser);

    /// <summary>
    /// Удаляет пользователя по его ID. Доступ зависит от роли.
    /// </summary>
    Task DeleteUserAsync(string userId);

    /// <summary>
    /// Возвращает список пользователей и флаг hasMore.
    /// Флаг Deletable зависит от роли и владения.
    /// </summary>
    /// <returns>Список пользователей.</returns>
    Task<(List<UserViewModel>, bool)> GetAllUsersAsync(int page, int pageSize = 10);

    /// <summary>
    /// Асинхронно получает информацию о пользователе.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя. Если не указан, будет возвращена информация о текущем пользователе.</param>
    /// <returns>Кортеж, содержащий объект <see cref="UserViewModel"/> с данными пользователя и список строк содержащий все роли.</returns>
    Task<(UserViewModel, List<string>)> GetUserInfoAsync(string? userId = null);

    /// <summary>
    /// Асинхронно получает информацию о пользователе по идентификатору статьи.
    /// </summary>
    /// <param name="articleId">Идентификатор статьи, для которой требуется получить информацию о пользователе.</param>
    /// <returns>Объект типа <see cref="UserViewModel"/>, содержащий информацию о пользователе.</returns>
    Task<UserViewModel> GetUserInfoByArticleIdAsync(int articleId);

    /// <summary>
    /// Асинхронно находит идентификаторы пользователей по заданному имени.
    /// </summary>
    /// <param name="name">Имя пользователя, по которому будет выполнен поиск.</param>
    /// <returns>Возвращает строку с идентификаторами пользователей или null, если пользователи не найдены.</returns>
    Task<string?> FindUserIdsByNameAsync(string name);
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