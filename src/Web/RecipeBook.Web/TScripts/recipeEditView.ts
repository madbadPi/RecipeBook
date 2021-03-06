﻿$(document).ready(function () {
    let fTasteStarsVote = new FiveStarsVote(Const.FIVE_STARS_VOTE_TASTE_RATE_ELEMENT_ID);
    fTasteStarsVote.startListenToVote();
    let fEasyStarsVote = new FiveStarsVote(Const.FIVE_STARS_VOTE_EASY_RATE_ELEMENT_ID);
    fEasyStarsVote.startListenToVote();

    let crImg = new CropImage(onImageCroped);

    let dragDropImg = new DragDropImage(Const.DRAG_DROP_IMAGE_BOX_ID, onImageDroped, onError);
    dragDropImg.startListen();

    let imageFileSelector = new ImageFileSelector(Const.DRAG_DROP_IMAGE_BOX_ID, onImageDroped, onError);
    imageFileSelector.startListen();

    let linkedId = $('#' + Const.DRAG_DROP_IMAGE_BOX_ID).data(Const.HTML_ATTRIBUTE_VALUE_KEY);
    let token = $("#keyForm input[name=__RequestVerificationToken]").val();

    function onImageDroped(data: string) {
        crImg.start(data);
    }

    function onError(error: string) {
    }

    function onImageCroped(reader: FileReader) {
        let base64data = reader.result;
        let data = new FormData();
        data.append(ImageDataKeyEnum.Image, base64data.toString());
        data.append(ImageDataKeyEnum.Type, ImageTypeEnum.Recipe);
        data.append(ImageDataKeyEnum.LinkedId, linkedId)
        $.ajax({
            url: Const.API_IMAGE_URL,
            method: Const.POST,
            data: data,
            processData: false,
            contentType: false,
            headers: { 'X-CSRF-TOKEN': token.toString() },
            success: function (data) {
                crImg.stop(data);
            },
            error: function (error) {
                if (error.status == Const.NO_AUTHENTICATION_ERROR_NUMBER) {
                    window.location.href = Const.IDENTITY_LOGIN_URL;
                }
                else {
                    //TODO show custom error msg
                }
            }
        });
    };
});