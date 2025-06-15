// Скрипт для удаления комментария
function deleteComment(commentId, articleId) {
    if (!confirm('Вы уверены, что хотите удалить этот комментарий?')) {
        return;
    }
    const token = getAntiForgeryToken();
    $.ajax({
        url: '/AccountManager/delete_comment',
        type: 'POST',
        headers: {
            "RequestVerificationToken": token
        },
        data: { id: commentId },
        success: function (response) {
            // Удаляем блок комментария из DOM
            var commentDiv = $(`.comment[data-comment-id='${commentId}']`);
            commentDiv.remove();

            // Проверяем, остались ли комментарии, и добавляем placeholder, если их нет
            var commentsSection = $(`#comments-section-${articleId}`);
            if (commentsSection.find('.comment').length === 0) {
                commentsSection.html('<p class="text-muted">Комментариев пока нет.</p>');
            }
        },
        error: function (xhr, status, error) {
            console.error('Ошибка:', xhr.responseText);
            alert('Произошла ошибка при удалении комментария.');
        }
    });
}

// Скрипт для добавления комментария
function addComment(articleId) {
    const commentText = $('#commentText-' + articleId).val().trim();
    if (!commentText) {
        alert('Введите текст комментария.');
        return;
    }

    const token = getAntiForgeryToken();
    $.ajax({
        url: '/AccountManager/create_comment',
        type: 'POST',
        headers: { "RequestVerificationToken": token },
        data: { ArticleId: articleId, Text: commentText },
        success: function (response) {
            // Вставляем новые комментарии
            const commentsSection = $(`#comments-section-${articleId}`);
            commentsSection.html(response);
            $('#commentText-' + articleId).val('');

            //  Удаляем все скрытые токены из секции комментариев
            commentsSection.find('input[name="__RequestVerificationToken"]').remove();

        },
        error: function (xhr) {
            console.error('Ошибка:', xhr.responseText);
        }
    });
}


// Скрипт для редактирования комментария
function editComment(articleId, commentId) {
    var commentDiv = $(".comment[data-comment-id='" + commentId + "']");
    var commentText = commentDiv.find('.comment-text');
    var oldText = commentText.text().trim();
    var dateInfo = commentDiv.find('.text-muted.small');
    const token = getAntiForgeryToken();

    // Проверяем, есть ли уже существующее поле редактирования
    var editField = commentDiv.find('textarea');
    if (editField.length > 0) {
        return;
    }

    // Показываем поле ввода
    editField = $('<textarea>', {
        class: 'form-control',
        cols: '40',
        rows: '3'
    }).val(oldText);

    // Создаем кнопки "Сохранить" и "Отмена"
    var saveBtn = $('<button>', {
        type: 'button',
        class: 'btn btn-primary my-2',
        text: 'Сохранить'
    });

    var cancelBtn = $('<button>', {
        type: 'button',
        class: 'btn btn-secondary my-2',
        text: 'Отмена'
    });

    // Заменяем текст комментария на поле ввода и кнопки
    commentText.replaceWith(editField);
    commentDiv.append(saveBtn).append(cancelBtn);

    // Обработчик сохранения комментария
    saveBtn.on('click', function () {
        var editedText = editField.val().trim();
        if (editedText !== '') {
            $.ajax({
                url: '/AccountManager/edit_comment',
                type: 'POST',
                headers: {
                    "RequestVerificationToken": token
                },
                data: { CommentId: commentId, Text: editedText, ArticleId: articleId },
                success: function (response) {
                    // Обновляем текст комментария
                    commentDiv.find('.comment-text').text(editedText);
                    // Возвращаем исходное состояние
                    var newCommentText = $('<p>', {
                        class: 'comment-text card-text',
                        style: 'white-space: pre-wrap;'
                    }).text(editedText);

                    editField.replaceWith(newCommentText);

                    // Форматируем текущую дату в формате dd.MM.yyyy HH:mm
                    var now = new Date();
                    var day = String(now.getDate()).padStart(2, '0');
                    var month = String(now.getMonth() + 1).padStart(2, '0');
                    var year = now.getFullYear();
                    var hours = String(now.getHours()).padStart(2, '0');
                    var minutes = String(now.getMinutes()).padStart(2, '0');
                    var formattedDate = `${day}.${month}.${year} ${hours}:${minutes}`;

                    // Обновляем дату комментария
                    var currentDateText = dateInfo.text();
                    var createdPart = currentDateText.split(' | Обновлено:')[0];
                    var newDateText = createdPart + ' | Обновлено: ' + formattedDate;
                    dateInfo.text(newDateText);

                    saveBtn.remove();
                    cancelBtn.remove();
                },
                error: function (xhr, status, error) {
                    console.error('Ошибка при сохранении комментария:', xhr.responseText);
                    alert('Произошла ошибка при сохранении комментария.');

                }
            });
        } else {
            alert('Введите текст комментария.');
        }
    });

    // Обработчик отмены редактирования
    cancelBtn.on('click', function () {
        // Возвращаем исходный текст комментария и удаляем кнопки
        editField.replaceWith(commentText.clone(true));
        saveBtn.remove();
        cancelBtn.remove();
    });
}
