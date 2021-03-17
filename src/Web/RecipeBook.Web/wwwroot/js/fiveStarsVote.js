class fiveStarsVote extends EventTarget {
    constructor(iName) {
        super();
        this.itemName = iName;
        this.initStars();
    }
    initStars() {
        let allElements = $('[id^="' + this.itemName + '"]').toArray();
        allElements.forEach(function (Element) {
            let starsElementData = $(Element).data('value');
            let dataArray = starsElementData.split(',');
            let tasteRate = dataArray[1];
            var stars = $(Element).children('li.star');
            for (let i = 0; i < stars.length; i++) {
                if (i < parseInt(tasteRate)) {
                    $(stars[i]).addClass('selected');
                }
                else {
                    $(stars[i]).removeClass('selected');
                }
            }
        });
    }
    startListenToVote() {
        let context = this;
        let allElements = $('[id^="' + this.itemName + '"]').toArray();
        allElements.forEach(function (Element) {
            var stars = $(Element).children('li.star');
            for (let i = 0; i < stars.length; i++) {
                stars[i].addEventListener('mouseover', (event) => { context.onStartMouseOver(event.target); }, false);
                stars[i].addEventListener('mouseout', (event) => { context.onStartMouseOut(event.target); }, false);
                stars[i].addEventListener('click', (event) => { context.onStarClick(event.target); }, false);
            }
        });
    }
    /* 1. Visualizing things on Hover - See next part for action on click */
    onStartMouseOver(currentElement) {
        let starElement = currentElement.parentNode;
        let onStar = parseInt($(starElement).data('value'), 10); // The star currently mouse on
        // Now highlight all the stars that's not after the current hovered star
        $(starElement).parent().children('li.star').each(function (index) {
            if (index < onStar) {
                $(this).addClass('hover');
            }
            else {
                $(this).removeClass('hover');
            }
        });
    }
    ;
    onStartMouseOut(currentElement) {
        let starElement = currentElement.parentNode;
        $(starElement).parent().children('li.star').each(function (e) {
            $(this).removeClass('hover');
        });
    }
    ;
    /* 2. Action to perform on click */
    onStarClick(currentElement) {
        let starElement = currentElement.parentNode;
        let onStar = parseInt($(starElement).data('value'), 10); // The star currently selected
        //POST
        let parentElementData = $(starElement).parent().data('value');
        let array = parentElementData.split(',');
        let recipeId = array[0];
        let token = $("#keyForm input[name=__RequestVerificationToken]").val();
        let data = new FormData();
        data.append("RecipeId", recipeId);
        if (this.itemName.indexOf('Taste') >= 0) {
            data.append("Type", "Taste");
        }
        else {
            data.append("Type", "Easy");
        }
        data.append("Value", onStar.toString());
        $.ajax({
            url: "/api/vote",
            data: data,
            processData: false,
            contentType: false,
            type: "POST",
            headers: { 'X-CSRF-TOKEN': token.toString() },
            success: (result) => {
                var stars = $(starElement).parent().children('li.star');
                var starsValueItem = $(starElement).parent().children('li.list-inline-item');
                if (this.itemName.indexOf('Taste') >= 0) {
                    starsValueItem.html('(Taste rate:' + result + ')');
                }
                else {
                    starsValueItem.html('(Easy rate:' + result + ')');
                }
                for (let i = 0; i < stars.length; i++) {
                    if (i < parseInt(result)) {
                        $(stars[i]).addClass('selected');
                    }
                    else {
                        $(stars[i]).removeClass('selected');
                        $(stars[i]).removeClass('hover');
                    }
                }
                this.dispatchEvent(new Event('voteSuccess'));
            },
            error: function (error) {
                if (error.status == 401) {
                    window.location.href = '/Identity/Account/Login';
                }
                else {
                    //TODO show custom error msg
                }
            }
        });
    }
    ;
}
//# sourceMappingURL=fiveStarsVote.js.map