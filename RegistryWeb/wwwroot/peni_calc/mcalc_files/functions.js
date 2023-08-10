function searchDocument(text) {
	var select = document.getElementById('doc-list');
	if (!select)
		return;
	var res = '';
	var alt = '';
	for (var i = 0; i < docs.length; i++) { var op = docs[i];
		if (!text || op.name.toLowerCase().indexOf(text.toLowerCase()) > -1) {
			var name = text? op.name.replace(text, '<b style="color:#E0292A">' + text + '</b>') : op.name;
			res += '<li><a href="договоры/образец-' + op.link + '/"><div class="doc-bg"><div class="doc-active" style="width:' + op.rating + 'px"></div></div>' + name + '</a></li>';
		}
	}

	select.innerHTML = res/* + '<br><br>' + alt*/;
}

function voteDoc(id, mark) {
	voteDocEx(id, mark, 'doc');
}

function commentSubmit(id, form) {
	commentSubmitEx(id, form, 'doc');
}

function getUserType(radio) {
	for (var i = 0; i < radio.length; i++)
		if (radio[i].checked)
			return parseInt(radio[i].value);
	return 0;
}

function commentSubmitEx(id, form, mod) {

	var err = false;
	var isAuth = form.auth != undefined;


	if (!isAuth) {
		var userType = getUserType(form.userType);
		var isNewUser = userType == 1;
		if (isNewUser) { // проверяем нового пользователя

			if (form.name.value == 'Ваше имя' || form.name.value == '') {
				$(form.name).addClass('error-field');
				err = true;
			} else
				$(form.name).removeClass('error-field');
			if (form.email.value == 'Ваш Email' || form.email.value == '') {
				$(form.email).addClass('error-field');
				err = true;
			} else
				$(form.email).removeClass('error-field');

			if (form.password.value == '') {
				$(form.password).addClass('error-field');
				err = true;
			} else
				$(form.password).removeClass('error-field');

			if (form.password.value != form.password2.value) {
				$(form.password2).addClass('error-field');
				err = true;
			} else
				$(form.password2).removeClass('error-field');

		} else {
			var isIs = userType == 3;
			if (isIs) { // проверяем незарегистрированного
				if (form.name_no.value == 'Ваше имя' || form.name_no.value == '') {
					$(form.name_no).addClass('error-field');
					err = true;
				} else
					$(form.name_no).removeClass('error-field');
			} else { // остаётся зарегистрированный
				if (form.email_r.value == 'Ваш Email' || form.email_r.value == '') {
					$(form.email_r).addClass('error-field');
					err = true;
				} else
					$(form.email_r).removeClass('error-field');

				if (form.password_r.value == '') {
					$(form.password_r).addClass('error-field');
					err = true;
				} else
					$(form.password_r).removeClass('error-field');
			}
		}
	}

	if (form.phone) {
		$(form.phone).removeClass('error-field');
		if ((form.phone.value == '') || (form.phone.value == 'Ваш телефон')) {
			$(form.phone).addClass('error-field');
			err = true;
		} else if (form.phone.value.length < 7 || form.phone.value.length > 20) {
			$(form.phone).addClass('error-field');
			err = true;
		}
	}


	$(form.message).removeClass('error-field');
	if (!form.message.value || form.message.value == 'Ваш комментарий') {
		$(form.message).addClass('error-field');
		err = true;
	} else
		$(form.message).removeClass('error-field');

	if (err)
		return;


	if (!isAuth && form.receive.checked && form.userType[0].checked) // незарегистрированный хочет подписаться
		if (!confirm('Без регистрации вы не сможете подписаться на комментарии.\n\nПродолжить?'))
			return;

	var params = getParameters(form);
	params += '&mod=' + mod + '&id=' + id;
	$.ajax({
		url : '/add-comment.ajax',
		type: 'post',
		data : params,
		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		error: function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				displayError(err, form);//alert(err[2]);
				return;
			}
			var data;
			try {
				data = eval('(' + response + ')');
			} catch (e) {
				alert('Ошибка: ' + response);
				return;
			}
			if (data.mark < 5) {
				var rating = data.marks/data.marksAmount;
				$('.star' + data.mark).addClass('star-selected');
				$('.star-voted').css('display', 'block');

				$('.doc-vote-info').html('<div class="doc-voted rating_' + Math.min(4, data.mark) + '" title="Средняя оценка документа">' + rating.toFixed(2).replace('.', ',') + '</div>');
				$('.dog-block .doc-active').css('width', rating*30 + 'px');
				$('#votesPane').html(data.mark);
			}

			$('#comments-pane').append(generateComment(data));
			$('#comment-bad').hide();
			$('#comment-posted').show();
			$('#no-comment').hide();
			//closeModal('#vote-com');
			if (form.name) form.name.value = 'Ваше имя';
			form.message.value = 'Ваш комментарий';
			if (form.email) form.email.value = 'Ваш Email';
			alert('Ваш комментарий добавлен');
		}
	});
}

function commentRatingNoteSubmit(form, mod) {
	var data = getParameters(form) + '&mod=' + mod;
	$.ajax({
		url : '/comment-rating-note.ajax',
		type : 'post',
		data : data,
		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},
		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				alert(err[2]);
				return;
			}
			if (response) {
				alert(response);
				return;
			}
			$('#noteAnswer').hide();
			alert('Спасибо!\n\nВаш отзыв добавлен.');
		}
	});
}

function voteDocEx(id, mark, mod) {
	markClick(mark);
	if (mark < 5) {
		//$('#comment-message').html('<span style="color:#cc0000; font-weight: bold; font-size:16px">Считаете документ неправильным?</span><br>Оставьте комментарий, и мы исправим недостатки.<br><b>Без комментария оценка не будет учтена!</b>');
		$('#comment-bad').show();
		$('html, body').animate({
			 scrollTop: $("#comment-message").offset().top - 100
		 }, 200);

		return;
	}

	$.ajax({
		url : '/vote.ajax?mark=' + mark + '&id=' + id + '&mod=' + mod,
		type: 'post',

		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				return;
			}

			var data = response.split(' ');
			var rating = data[0]/data[1];
			$('.star' + mark).addClass('star-selected');
			$('.star-voted').css('display', 'block');

			$('.doc-vote-info').html('<div class="doc-voted rating_' + Math.min(4, mark) + '" title="Средняя оценка документа">' + rating.toFixed(2).replace('.', ',') + '</div>');
			$('.dog-block .doc-active').css('width', rating*30 + 'px');
			$('#votesPane').html(data[1]);
			$('#comment-bad').hide();
			//$('#comment-message').html('<span style="font-size:16px; font-weight: bold">Спасибо, ваша оценка учтена.</span><br>Также вы можете оставить комментарий к своей оценке.');
			$('#comment-posted').show();
			$('html, body').animate({
				 scrollTop: $("#comment-message").offset().top - 100
			 }, 200);
		}
	});
}

function displayError(err, form) {
	var code = parseInt(err[1]);
	var response = err[2];
	switch (code) {
		case 1 : case 100 : alert(response); break;
		default :
			var data = eval('(' + response + ')');
			for (d in data) {
				for (i = 0; i < form.elements.length; i++) {
					if (form.elements[i].name == d) {
						$(form.elements[i]).addClass('error-field');
						break;
					}
				}

			}

	}
}

function htmlspecialchars(html) {
      // Сначала необходимо заменить &
      html = html.replace(/&/g, "&amp;");
      // А затем всё остальное в любой последовательности
      html = html.replace(/</g, "&lt;");
      html = html.replace(/>/g, "&gt;");
      html = html.replace(/"/g, "&quot;");
      // Возвращаем полученное значение
      return html;
}

function generateComment(data) {
	return data.message;
}

function docRequestSubmit(form) {

	var err = false;
	$(form.name).removeClass('error-field');
	if (!form.name.value || form.name.value == 'Ваше имя') {
		$(form.name).addClass('error-field');
		err = true;
	}

	$(form.message).removeClass('error-field');
	if (!form.message.value || form.message.value == 'Ваш комментарий') {
		$(form.message).addClass('error-field');
		err = true;
	}

	$(form.email).removeClass('error-field');
	if (!form.email.value || form.email.value == 'Ваш E-mail') {
		$(form.email).addClass('error-field');
		err = true;
	}

	if (err)
		return;


	var params = getParameters(form);
	$.ajax({
		url : '/add-document-request.ajax',
		type: 'post',
		data : params,
		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		error: function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				if (err[1] == 1) {
					var data = eval('(' + err[2] + ')');
					for (var d in data)
						$(form.elements[d]).addClass('error-field');
				} else {
					alert(err[2]);

				}
				return;
			} else if (response) {
				alert('Ошибка:' + response);
				return;
			}

			$('#doc-not').css('display', 'none');
			form.name.value = 'Ваше имя';
			form.message.value = 'Ваш комментарий';
			form.email.value = 'Ваш E-mail';
			alert('Ваша заявка отправлена.\nЖдите ответа на указанный E-mail');
		}
	});
}

function getParameters(form) {
	if (!form)
		return "";
	var res = "";
	var fields = form.getElementsByTagName("input");
	for (var i = 0; i < fields.length; i++) {
		var f = fields[i];
		if (!f.name) continue;
		if ((f.type == "checkbox" || f.type == "radio") && !f.checked) continue;
		res += "&" + f.name + "=" + encodeURIComponent(f.value);
	}
	fields = form.getElementsByTagName("textarea");
	for (i = 0; i < fields.length; i++) {
		f = fields[i];
		if (f.name)
			res += "&" + f.name + "=" + encodeURIComponent(f.value);
	}

	fields = form.getElementsByTagName("select");
	for (i = 0; i < fields.length; i++) {
		f = fields[i];
		if (f.name)
			res += "&" + f.name + "=" + encodeURIComponent(f.options[f.selectedIndex].value);
	}
	return res.substring(1);
}

function checkErrorResponse(response) {
	return /^ERROR:(\d+) (.+)/.exec(response);
}

function ctrlEnter(event, formElem) {
    if((event.ctrlKey) && ((event.keyCode == 0xA) || (event.keyCode == 0xD))) {
		if (formElem.onsubmit) {
			if (formElem.onsubmit())
				formElem.submit();
		} else
			formElem.submit();
	}
}

function showModal(elm) {
	$(elm).show();
	addEvent(window, "keypress", actions[elm]);
}

function closeVote() {
	if (!(document.voteForm.name.value == '' || document.voteForm.name.value == 'Ваше имя')
		|| 	!(document.voteForm.message.value == '' || document.voteForm.message.value == 'Ваш комментарий')
	) {
		if (!confirm('Вы уверены, что хотите закрыть форму комментирования?'))
			return;
	}

	closeModal('#vote-com');
}

function closeModal(elm) {
	$(elm).hide();
	removeEvent(window, "keypress", actions[elm]);
}

var actions = {
'#doc-not' : function (e) {
	if (e.keyCode == 27)
		closeModal('#doc-not');
},

'#vote-com' : function (e) {
	if (e.keyCode == 27)
		closeVote();
}
};


function addEvent(elem, type, handler) {
	if (elem.addEventListener) {
		elem.addEventListener(type, handler, false)
	}
	else {
		elem.attachEvent("on" + type, handler)
	}
}

function removeEvent(elem, type, handler) {

	if (elem.removeEventListener) {
		elem.removeEventListener(type, handler, false)
	}

	else {
		elem.detachEvent("on" + type, handler)
	}
}

function yesComment(id, mod, isJuridic) {
	rateComment(id, +1, mod, isJuridic);
}

function noComment(id, mod, isJuridic) {
	rateComment(id, -1, mod, isJuridic);
}

function rateComment(id, val, mod, isJuridic) {

	$.ajax({
		url : '/comment-rating.ajax',
		type: 'post',
		data : "id=" + id + '&val=' + val + '&mod=' + mod,
		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		error: function() {

		},

		success: function (response) {

			var err = checkErrorResponse(response);
			if (err) {
				$('#noteAnswer').hide();
				alert(err[2]);
				return;
			}

			var newMark = eval('(' + response + ')');

			var span = $('#rating' + id + ' span');
			span.text(newMark > 0? '+' + newMark : newMark);
			span.attr('class', '');
			span.addClass(newMark > 0? 'cr-plus' : (newMark < 0? 'cr-minus' : 'cr-zero'));
			if (val > 0)
				$('#yesComment' + id).addClass('active');
			else
				$('#noComment' + id).addClass('active');

			//if (isJuridic) {
				var form = $('#noteAnswer');
				if (form.length == 0) return;
				document.noteAnswerForm.rating.value = val;
				document.noteAnswerForm.id.value = id;
				document.noteAnswerForm.message.value = '';
				var offset = $('#rating' + id).offset();
				var h3 = $('#noteAnswer h3');
				if (val > 0) {
					h3.html('<img style="margin-top:2px" src="/images/vote_yes1.gif"> Вы оценили юриста положительно');
					h3.css('background', 'green');
				}
				else {
					h3.html('<img style="margin-top:4px" src="/images/vote_no1.gif"> Вы оценили юриста отрицательно');
					h3.css('background', '#cc0000');
				}
				form.css({top: offset.top - form.height() - 30, left: offset.left - form.width()});
				form.fadeIn();

				var aClick = function() {
					form.fadeOut();
					$(document).unbind('click', aClick);
				};

				$(document).click(aClick);
			//}

		}
	});
	

}

var nextSearch = null;
var searchInProcess = false;

function searchItems(text) {
	if (searchInProcess) {
		nextSearch = text;
	} else {
		searchInProcess = true;
		$.ajax({
			url : '/search-suggest.ajax',
			type: 'post',
			data : "text=" + text,
			beforeSend : function () {

				},
			complete : function() {
				searchInProcess = false;
				if (nextSearch) {
					var n = nextSearch;
					nextSearch = null;
					searchItems(n);
				}
			},

			error: function() {

			},

			success: function (response) {
				var err = checkErrorResponse(response);
				if (err) {
					alert(err[2]);
					return;
				}

				var data = eval('(' + response + ')');
				var res = '';
				for (var i = 0; i < data.length; i++)
					res += '<li><a href="' + data[i].url + '">' + data[i].name + '</a></li>';
				if (res) {
					$('#search-suggest ul').html(res);
					if ($('#search-suggest').css('display') == 'none') {
						addEvent(window, 'click', hideSuggest);
						addEvent(window, 'keypress', suggestEscape);
						$('#search-suggest').show();
					}
				} else
					hideSuggest();
			}
		});
	}
}

function suggestEscape(e) {
	if (e.keyCode == 27)
		hideSuggest();
}

function hideSuggest() {
	removeEvent(window, 'click', hideSuggest);
	$('#search-suggest').hide();
}

function searchCodex(text) {
	searchListEx(text, 'codex-list', true);
}

function searchListEx(text, id, isTree) {
	text = text.toLowerCase();
	$('#' + id + ' li > a').each(function() {
		var name = this.innerHTML.replace('<b style="color:#E0292A">', '').replace('</b>', '');
		if (!text || name.toLowerCase().indexOf(text) > -1) {
			name = text? name.replace(text, '<b style="color:#E0292A">' + text + '</b>') : name;
			this.innerHTML = name;
			this.parentNode.style.display = 'list-item';
		} else {
			this.parentNode.style.display = 'none';
		}
	});
	if (isTree)
		$('#' + id + ' a').each(function() {
			if (this.parentNode.style.display != this.parentNode.parentNode.parentNode.style.display)
				hideEmptyCodexChapters(this, id);
		});
}

function markClick(mark) {
	if (document.commentForm.mark.value == mark)
		return;
	$('#stMark').css({width: (mark*30) + 'px'});
	document.commentForm.mark.value = mark;
}

function hideEmptyCodexChapters(anchor, id) {
	         // a   li         ul
	var ul = anchor.parentNode.parentNode;
	while (ul.id != id) {
		var isVisible = false;
		for (var i = 0; i < ul.childNodes.length; i++) {
			var ch = ul.childNodes[i];
			if (ch.style && ch.style.display != 'none') {
				isVisible = true;
				break;
			}
		}
		var prev = ul.parentNode.style.display;
		ul.parentNode.style.display = isVisible? 'block' : 'none';
		if (prev == ul.parentNode.style.display)
			return;
		ul = ul.parentNode.parentNode;
	}
}

function add_favorite(a) {
	var title = document.title.substr(0, 120);
	var url = document.location;

	/*$.ajax({
		url : '/bookmark.ajax?title=' + encodeURIComponent(title) + '&url=' + encodeURIComponent(url)
	});*/

	if (document.all) {
		window.external.AddFavorite(url, title);
		// Internet Explorer
	}

	try {
		// Mozilla
		window.sidebar.addPanel(title, url, "");
	}
	catch (e) {
		// Opera
		if (typeof(opera) == "object") {
			a.title = title;
			a.href = url;
			a.rel = "sidebar";
			return true;
		}
		else {
			// Unknown
			alert('Нажмите Ctrl-D чтобы добавить страницу в закладки');
		}
	}
	return false;
}

function commentTo(id, name) {
	document.commentForm.id_to.value = id;
	$('#commentToName').text(name);
	$('#commentTo').show();
	$('html, body').animate({scrollTop: $(document.commentForm).offset().top - 50}, 200);
	
}

function commentToCancel() {
	document.commentForm.id_to.value = '';
	$('#commentTo').hide();
}

function moveToComment(idTo) {
	var comItem = $('#com' + idTo);
	comItem.css('background', '#EDEAEA');
	setTimeout("$('#com" + idTo + "').css('background', '#fcfcfc');", 2000);
	$('html, body').animate({scrollTop: comItem.offset().top - 50}, 200);
}

function toggleMap(a, id) {

	a = $('#' + a);
	var action = a.attr('d');
	if (!action)
		action = 'd';
	var el = $('#' + id);
	if (action == 'd') {
		el.css('height', 400);
		a.attr('d', 'u');
		a.html('<img src="/images/map_up.png">');
	} else {
		el.css('height', 200);
		a.attr('d', 'd');
		a.html('<img src="/images/map_down.png">');
	}
	map.container.fitToViewport();
}


function hideAddons() {
	$.cookie('addons', 'hide', {path : '/', expires : 60});
	$('#banner').hide();
}

/*function hideNotFound() {
	$.cookie('dnf', 'hide', {path : '/', expires : 10000});
	$('#document-not-found').hide();
}*/

function placeAds(from, to) {
	from = document.getElementById(from);
	document.getElementById(to).innerHTML = from.innerHTML;
	from.parentNode.removeChild(from);
}

function expandSearchTab(id, exp) {
	document.getElementById(id).style.maxHeight = 'none';
	document.getElementById(exp).style.display = 'none';
}

function questionMarkBest(commentId, path) {
	$.ajax({
		url : '/questions/' + path + '.ajax',
		type: 'get',
		data : "id=" + commentId,
		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		error: function() {
			alert('Возникла ошибка связи');
		},

		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				alert(err[2]);
				return;
			}

			var votesLeft = parseInt(response);
			var el = $('#vmarker' + commentId);
			el.html('Отмечен лучшим');
			el.addClass('qc-marked-best');
			el.removeClass('qc-make-best');
			if (votesLeft == 0)
				$('.qc-make-best').hide();
		}
	});
}

function expertMarkBest(commentId) {
	questionMarkBest(commentId, 'expert-vote');
}

function creatorMarkBest(commentId) {
	questionMarkBest(commentId, 'creator-vote');
}

function expertMarkPlagiat(commentId) {
	questionMarkPlagiat(commentId);
}

function questionMarkPlagiat(commentId) {
	$.ajax({
		url : '/questions/plagiat.ajax',
		type: 'get',
		data : "id=" + commentId,
		beforeSend : function () {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.show();
			},
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		error: function() {
			alert('Возникла ошибка связи');
		},

		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				alert(err[2]);
				return;
			}
			var el = $('#plagiat_' + commentId);
			if (el.hasClass('qc-plagiat')) {
				el.find('a').html('не плагиат');
				el.removeClass('qc-plagiat');
			} else {
				el.find('a').html('это плагиат');
				el.addClass('qc-plagiat');

			}
		}
	});
}

function codexInfoOpen(uid) {
	if (document.getElementById('ci' + uid).style.display == 'inline')
		document.getElementById('ci' + uid).style.display = 'none';
	else
		document.getElementById('ci' + uid).style.display = 'inline';
}

function globalScrollHandler() {
	if ($(window).width() > 800) {
		var scrollTop = $(window).scrollTop();

		var topDisplay = $('#toTop').css('display') == 'none';
		if (scrollTop > 300 && topDisplay) {
			$('#toTop').fadeIn(200);
		} else if (scrollTop == 0 && !topDisplay) {
			$('#toTop').fadeOut(200);
		}
	}
}

function initCopyDocument() {
	var aClick = function(e) {
		var copied = $('#copiedNote');
		copied.fadeIn();
		$(".dogovor_content").unbind('copy', aClick);
		var cClick = function () {
			copied.fadeOut();
			$(document).unbind('click', cClick);
		};
		$(document).bind('click', cClick);
	};
	$(".dogovor_content").bind('copy', aClick);
}

function updateQuote() {
	$.ajax({
		url : '/quote.ajax',
		complete : function() {
			var loadBlock = $("#ajaxLoad");
			if (loadBlock.length)
				loadBlock.hide();
		},

		success: function (response) {
			var err = checkErrorResponse(response);
			if (err) {
				alert(err[2]);
				return;
			}

			$('#dq-conten').text(response);
		}
	});
}

function hoverSound(id) {
	var el = document.getElementById(id);
	el.pause();
	el.currentTime = 0;
	el.play();
}

function checkEvent(e, need) {
	var event = e || window.event;
	var target = event.target || event.srcElement;
	return need == target;
}

var curBannerPosition;
var BANNERS_AMOUNT = 12;
var BANNER_WIDTH = 904;
var mBannerTimer;

function moveBanner(direction) {
	var newPosition = curBannerPosition + direction;
	if (newPosition < 0)
		newPosition = BANNERS_AMOUNT - 1;
	else if (newPosition >= BANNERS_AMOUNT)
		newPosition = 0;
	curBannerPosition = newPosition;
	$('#calc-pane2').animate({marginLeft: -curBannerPosition*BANNER_WIDTH + 'px'}, 200);

}

function moveBanner2(direction) {
	moveBanner(direction);
	clearInterval(mBannerTimer);
	mBannerTimer = setInterval(function() {
		moveBanner(1);
	}, 20000);
}

if (typeof copyToClipboard == 'undefined') {
	copyToClipboard = function(elementId) {
		fnSelect(elementId);
		var res = document.execCommand("copy");
		fnDeSelect();
		return res;
	};

	fnSelect = function (objId) {
		fnDeSelect();
		if (document.selection) {
			var range = document.body.createTextRange();
			range.moveToElementText(document.getElementById(objId));
			range.select();
		}
		else if (window.getSelection) {
			var range = document.createRange();
			range.selectNode(document.getElementById(objId));
			window.getSelection().addRange(range);
		}
	};

	fnDeSelect = function() {
		if (document.selection)
			document.selection.empty();
		else if (window.getSelection)
			window.getSelection().removeAllRanges();
	};
}

function copyTextToClipboard(text) {
	var textArea = document.createElement("textarea");
	textArea.value = text;
	textArea.style.height = '1px';
	document.body.appendChild(textArea);
	textArea.select();
	var successful = document.execCommand('copy');
	document.body.removeChild(textArea);
	return successful;
}

function copyCodexPartToClipboard(href) {
	if (copyTextToClipboard(href))
		alert('Ссылка на часть скопирована');
	else
		window.prompt('Не удалось скопировать ссылку на часть. Нажмите Ctrl+C самостоятельно.', href);
	return false;
}