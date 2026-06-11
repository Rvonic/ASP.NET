const sidebar = document.getElementById("ux-sidebar");
const mobileToggle = document.querySelector(".ux-mobile-toggle");

if (sidebar && mobileToggle) {
	mobileToggle.addEventListener("click", () => {
		const isOpen = sidebar.classList.toggle("ux-sidebar-open");
		mobileToggle.setAttribute("aria-expanded", String(isOpen));
	});

	document.addEventListener("click", (event) => {
		const target = event.target;
		if (!(target instanceof Node)) {
			return;
		}

		const clickedInsideSidebar = sidebar.contains(target) || mobileToggle.contains(target);
		if (!clickedInsideSidebar && window.innerWidth <= 820) {
			sidebar.classList.remove("ux-sidebar-open");
			mobileToggle.setAttribute("aria-expanded", "false");
		}
	});
}

function runPageCarAnimation() {
	if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
		return;
	}

	const car = document.createElement("div");
	car.className = "ux-page-car";
	car.setAttribute("aria-hidden", "true");
	car.innerHTML = `
		<div class="ux-page-car-shadow"></div>
		<div class="ux-page-car-body">
			<span class="ux-page-car-hood"></span>
			<span class="ux-page-car-roof"></span>
			<span class="ux-page-car-window ux-page-car-window-front"></span>
			<span class="ux-page-car-window ux-page-car-window-back"></span>
			<span class="ux-page-car-door"></span>
			<span class="ux-page-car-handle"></span>
			<span class="ux-page-car-mirror"></span>
			<span class="ux-page-car-spoiler"></span>
			<span class="ux-page-car-light"></span>
			<span class="ux-page-car-rs">RS</span>
		</div>
		<span class="ux-page-car-wheel ux-page-car-wheel-left"></span>
		<span class="ux-page-car-wheel ux-page-car-wheel-right"></span>
	`;

	document.body.appendChild(car);

	let tireFrame = 0;
	let tireTrailActive = true;
	const createTireMarkSegment = () => {
		if (!tireTrailActive || !document.body.contains(car)) {
			return;
		}

		tireFrame += 1;
		if (tireFrame % 4 !== 0) {
			window.requestAnimationFrame(createTireMarkSegment);
			return;
		}

		const carRect = car.getBoundingClientRect();
		const tireMarks = document.createElement("span");
		tireMarks.className = "ux-page-tire-marks";
		tireMarks.setAttribute("aria-hidden", "true");
		tireMarks.style.left = `${carRect.left + 34}px`;
		tireMarks.style.top = `${carRect.bottom - 15}px`;
		document.body.appendChild(tireMarks);
		tireMarks.addEventListener("animationend", () => tireMarks.remove(), { once: true });
		window.requestAnimationFrame(createTireMarkSegment);
	};

	let smokeFrame = 0;
	let smokeTrailActive = true;
	let smokeTrailStarted = false;
	const createSmokePuff = () => {
		if (!smokeTrailActive || !document.body.contains(car)) {
			return;
		}

		if (!smokeTrailStarted) {
			window.requestAnimationFrame(createSmokePuff);
			return;
		}

		smokeFrame += 1;
		if (smokeFrame % 5 !== 0) {
			window.requestAnimationFrame(createSmokePuff);
			return;
		}

		const carRect = car.getBoundingClientRect();
		const puff = document.createElement("span");
		const puffVariant = (smokeFrame / 5) % 3;
		puff.className = `ux-page-smoke-puff ux-page-smoke-puff-${puffVariant}`;
		puff.setAttribute("aria-hidden", "true");
		puff.style.left = `${carRect.left + 8}px`;
		puff.style.top = `${carRect.bottom - 24}px`;

		document.body.appendChild(puff);
		puff.addEventListener("animationend", () => puff.remove(), { once: true });
		window.requestAnimationFrame(createSmokePuff);
	};

	window.setTimeout(() => {
		tireTrailActive = false;
	}, 1780);
	window.setTimeout(() => {
		smokeTrailStarted = true;
	}, 1700);
	window.requestAnimationFrame(createTireMarkSegment);
	window.requestAnimationFrame(createSmokePuff);
	car.addEventListener("animationend", () => {
		smokeTrailActive = false;
		car.remove();
	}, { once: true });
}

runPageCarAnimation();

let customValidationRulesRegistered = false;

function registerCustomValidationRules() {
	if (customValidationRulesRegistered) {
		return;
	}

	if (!window.jQuery || !window.jQuery.validator) {
		return;
	}

	const validator = window.jQuery.validator;
	const unobtrusive = window.jQuery.validator.unobtrusive;

	validator.addMethod("uxselectedrequired", function (value, element, targetSelector) {
		const hiddenValue = window.jQuery(targetSelector).val()?.toString().trim() ?? "";
		const visibleValue = value?.toString().trim() ?? "";
		return hiddenValue.length > 0 || visibleValue.length === 0;
	});

	if (unobtrusive) {
		unobtrusive.adapters.addSingleVal("uxselectedrequired", "target");
	}

	validator.addMethod("uxdatetimevalue", function (value, element, targetSelector) {
		const hiddenValue = window.jQuery(targetSelector).val()?.toString().trim() ?? "";
		const visibleValue = value?.toString().trim() ?? "";
		return hiddenValue.length > 0 || visibleValue.length === 0;
	});

	if (unobtrusive) {
		unobtrusive.adapters.addSingleVal("uxdatetimevalue", "target");
	}

	customValidationRulesRegistered = true;
}

if (document.readyState === "loading") {
	document.addEventListener("DOMContentLoaded", registerCustomValidationRules);
} else {
	registerCustomValidationRules();
}

window.addEventListener("load", registerCustomValidationRules);

class DateTimeControl {
	constructor(root) {
		this.root = root;
		this.displayInput = root.querySelector("[data-ux-datetime-display]");
		this.valueInput = root.querySelector("[data-ux-datetime-value]");
		this.toggleButton = root.querySelector("[data-ux-datetime-toggle]");
		this.popup = root.querySelector("[data-ux-datetime-popup]");
		this.grid = root.querySelector("[data-ux-datetime-grid]");
		this.weekdays = root.querySelector("[data-ux-datetime-weekdays]");
		this.monthTitle = root.querySelector("[data-ux-datetime-month-title]");
		this.yearSelect = root.querySelector("[data-ux-datetime-year]");
		this.prevButton = root.querySelector("[data-ux-datetime-prev]");
		this.nextButton = root.querySelector("[data-ux-datetime-next]");
		this.hourInput = root.querySelector("[data-ux-datetime-hour]");
		this.minuteInput = root.querySelector("[data-ux-datetime-minute]");
		this.todayButton = root.querySelector("[data-ux-datetime-today]");
		this.nowButton = root.querySelector("[data-ux-datetime-now]");
		this.clearButton = root.querySelector("[data-ux-datetime-clear]");
		this.doneButton = root.querySelector("[data-ux-datetime-done]");
		this.errorMessage = root.querySelector("[data-ux-datetime-error]");
		this.requiredMessage = root.querySelector("[data-ux-datetime-required]");
		this.timeRow = root.querySelector("[data-ux-datetime-time-row]");
		this.requiredText = root.dataset.uxDatetimeRequiredMessage || "This field is required.";
		this.invalidText = root.dataset.uxDatetimeInvalidMessage || "Enter a valid date.";
		this.showTime = (root.dataset.uxDatetimeShowTime || "false").toLowerCase() === "true";
		// Server sets document.lang from the browser's Accept-Language header.
		const detectedLang = (document.documentElement.lang || navigator.languages?.[0] || navigator.language || "").toLowerCase();
		const pickLocale = (lang) => {
			if (!lang) return "hr"; // default
			if (lang.startsWith("en")) return "en";
			return "hr"; // fallback to Croatian for other locales
		};
		this.locale = pickLocale(detectedLang);
		this.monthFormatter = new Intl.DateTimeFormat(this.locale, { month: "long", year: "numeric" });
		this.weekdayFormatter = new Intl.DateTimeFormat(this.locale, { weekday: "short" });
		this.dateFormatter = new Intl.DateTimeFormat(this.locale, this.showTime
			? {
				year: "numeric",
				month: "2-digit",
				day: "2-digit",
				hour: "2-digit",
				minute: "2-digit",
				hourCycle: "h23"
			}
			: {
				year: "numeric",
				month: "2-digit",
				day: "2-digit"
			});
		this.order = this.resolveDateOrder();
		this.firstDay = this.resolveFirstDay();
		this.value = this.parseStoredValue(this.valueInput.value);
		this.viewDate = this.value ? new Date(this.value) : new Date();

		if (this.value) {
			this.displayInput.value = this.formatDisplay(this.value);
		} else {
			this.displayInput.value = "";
		}

		this.renderWeekdays();
		this.renderCalendar();
		this.syncTimeInputs();
		if (!this.showTime && this.timeRow) {
			this.timeRow.hidden = true;
		}
		this.bindEvents();
	}

	bindEvents() {
		this.toggleButton.addEventListener("click", () => this.togglePopup());
		this.prevButton.addEventListener("click", () => this.shiftMonth(-1));
		this.nextButton.addEventListener("click", () => this.shiftMonth(1));
		this.yearSelect?.addEventListener("input", () => this.changeYear(false));
		this.yearSelect?.addEventListener("change", () => this.changeYear(true));
		this.yearSelect?.addEventListener("blur", () => this.changeYear(true));
		this.yearSelect?.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.changeYear(true);
			}
		});
		this.todayButton.addEventListener("click", () => this.pickDate(new Date()));
		this.nowButton.addEventListener("click", () => this.pickDate(new Date(), true));
		this.clearButton.addEventListener("click", () => this.clear());
		this.doneButton.addEventListener("click", () => this.commitDisplay(true));
		this.displayInput.addEventListener("focus", () => this.openPopup());
		this.displayInput.addEventListener("input", () => this.clearValidation());
		// Delay committing on blur so clicks inside the popup (day buttons)
		// can run first and set the value. If the newly focused element is
		// still inside this control, skip the commit.
		this.displayInput.addEventListener("blur", () => {
			window.setTimeout(() => {
				try {
					const active = document.activeElement;
					if (this.root.contains(active)) {
						// Focus moved inside the picker; don't commit yet.
						return;
					}
				} catch (e) {
					// ignore
				}
				this.commitDisplay(false);
			}, 0);
		});
		this.displayInput.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.commitDisplay(true);
				this.closePopup();
			}
			if (event.key === "Escape") {
				this.closePopup();
			}
		});
		if (this.showTime) {
			this.hourInput.addEventListener("change", () => this.updateTimeFromInputs());
			this.minuteInput.addEventListener("change", () => this.updateTimeFromInputs());
		}
		this.root.addEventListener("click", (event) => this.handleRootClick(event));
		document.addEventListener("click", (event) => this.handleDocumentClick(event));
		document.addEventListener("keydown", (event) => {
			if (event.key === "Escape") {
				this.closePopup();
			}
		});
		this.root.closest("form")?.addEventListener("submit", () => this.commitDisplay(true));
	}

	handleRootClick(event) {
		const target = event.target;
		if (!(target instanceof Element)) {
			return;
		}

		const dayButton = target.closest("[data-ux-datetime-day]");
		if (dayButton) {
			const year = Number(dayButton.getAttribute("data-year"));
			const month = Number(dayButton.getAttribute("data-month"));
			const day = Number(dayButton.getAttribute("data-day"));
			this.pickDate(new Date(year, month, day), false);
		}
	}

	handleDocumentClick(event) {
		const target = event.target;
		if (!(target instanceof Node)) {
			return;
		}

		if (!this.root.contains(target)) {
			this.closePopup();
		}
	}

	resolveDateOrder() {
		const parts = new Intl.DateTimeFormat(this.locale, { year: "numeric", month: "numeric", day: "numeric" }).formatToParts(new Date(2020, 10, 23));
		return parts.filter((part) => part.type === "year" || part.type === "month" || part.type === "day").map((part) => part.type);
	}

	resolveFirstDay() {
		try {
			const locale = new Intl.Locale(this.locale);
			if (locale.weekInfo && typeof locale.weekInfo.firstDay === "number") {
				return locale.weekInfo.firstDay % 7;
			}
		} catch {
			return 1;
		}

		return 1;
	}

	renderWeekdays() {
		const weekdayLabels = [];
		for (let dayIndex = 0; dayIndex < 7; dayIndex += 1) {
			const date = new Date(2020, 10, 1 + ((this.firstDay + dayIndex) % 7));
			weekdayLabels.push(`<span>${this.weekdayFormatter.format(date)}</span>`);
		}

		this.weekdays.innerHTML = weekdayLabels.join("");
	}

	parseStoredValue(value) {
		if (!value) {
			return null;
		}

		if (this.showTime) {
			const isoDateTime = value.match(/^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2}))?$/);
			if (isoDateTime) {
				const [, year, month, day, hour, minute, second = "0"] = isoDateTime;
				const parsed = new Date(
					Number(year),
					Number(month) - 1,
					Number(day),
					Number(hour),
					Number(minute),
					Number(second),
					0
				);
				return this.isValidCandidate(parsed, Number(year), Number(month), Number(day)) ? parsed : null;
			}
		}

		const parts = value.split("-").map((part) => Number(part));
		if (parts.length !== 3 || parts.some((part) => Number.isNaN(part))) {
			return null;
		}

		const [year, month, day] = parts;
		const parsed = new Date(year, month - 1, day);
		return this.isValidCandidate(parsed, year, month, day) ? parsed : null;
	}

	formatDisplay(date) {
		return this.dateFormatter.format(date);
	}

	serialize(date) {
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, "0");
		const day = String(date.getDate()).padStart(2, "0");
		if (!this.showTime) {
			return `${year}-${month}-${day}`;
		}

		const hour = String(date.getHours()).padStart(2, "0");
		const minute = String(date.getMinutes()).padStart(2, "0");
		const second = String(date.getSeconds()).padStart(2, "0");
		return `${year}-${month}-${day}T${hour}:${minute}:${second}`;
	}

	formatCalendarDay(date) {
		return new Intl.DateTimeFormat(this.locale, { day: "numeric" }).format(date);
	}

	renderCalendar() {
		const year = this.viewDate.getFullYear();
		const month = this.viewDate.getMonth();
		const startDate = new Date(year, month, 1);
		const startOffset = (startDate.getDay() - this.firstDay + 7) % 7;
		const firstVisibleDate = new Date(year, month, 1 - startOffset);

		this.monthTitle.textContent = this.monthFormatter.format(startDate);
		if (this.yearSelect) {
			this.yearSelect.value = String(year);
			this.yearSelect.min = "1900";
			this.yearSelect.max = "2100";
		}

		const cells = [];
		for (let index = 0; index < 42; index += 1) {
			const cellDate = new Date(firstVisibleDate);
			cellDate.setDate(firstVisibleDate.getDate() + index);
			const inCurrentMonth = cellDate.getMonth() === month;
			const isSelected = this.value && this.isSameDate(cellDate, this.value);
			cells.push(`
				<button type="button"
					class="ux-datetime-day${inCurrentMonth ? "" : " ux-datetime-day-muted"}${isSelected ? " ux-datetime-day-selected" : ""}"
					data-ux-datetime-day
					data-year="${cellDate.getFullYear()}"
					data-month="${cellDate.getMonth()}"
					data-day="${cellDate.getDate()}"
					aria-label="${this.formatDisplay(cellDate)}">
					${this.formatCalendarDay(cellDate)}
				</button>`);
		}

		this.grid.innerHTML = cells.join("");
	}

	isSameDate(left, right) {
		return left.getFullYear() === right.getFullYear()
			&& left.getMonth() === right.getMonth()
			&& left.getDate() === right.getDate();
	}

	openPopup() {
		this.popup.hidden = false;
		this.displayInput.setAttribute("aria-expanded", "true");
		this.toggleButton.setAttribute("aria-expanded", "true");
		this.root.classList.add("ux-datetime-open");
		this.renderCalendar();
		this.syncTimeInputs();
	}

	closePopup() {
		this.popup.hidden = true;
		this.displayInput.setAttribute("aria-expanded", "false");
		this.toggleButton.setAttribute("aria-expanded", "false");
		this.root.classList.remove("ux-datetime-open");
	}

	togglePopup() {
		if (this.popup.hidden) {
			this.openPopup();
		} else {
			this.closePopup();
		}
	}

	shiftMonth(delta) {
		this.viewDate = new Date(this.viewDate.getFullYear(), this.viewDate.getMonth() + delta, 1);
		this.renderCalendar();
	}

	changeYear(finalize) {
		if (!this.yearSelect) {
			return;
		}

		const raw = this.yearSelect.value.trim();
		if (!raw) {
			return;
		}

		if (!finalize && raw.length < 4) {
			return;
		}

		const selectedYear = Number.parseInt(raw, 10);
		if (Number.isNaN(selectedYear)) {
			return;
		}

		const clampedYear = Math.min(2100, Math.max(1900, selectedYear));
		if (clampedYear !== selectedYear) {
			this.yearSelect.value = String(clampedYear);
		}

		const month = this.viewDate.getMonth();
		const maxDay = new Date(clampedYear, month + 1, 0).getDate();
		const day = Math.min(this.viewDate.getDate(), maxDay);
		this.viewDate = new Date(clampedYear, month, day);
		if (this.value) {
			const selectedValue = new Date(this.value);
			selectedValue.setFullYear(clampedYear);
			const adjustedDay = Math.min(selectedValue.getDate(), maxDay);
			selectedValue.setDate(adjustedDay);
			this.setValue(selectedValue);
		}
		this.renderCalendar();
	}

	pickDate(date, keepTime = false) {
		if (this.showTime && this.value && keepTime) {
			date.setHours(this.value.getHours(), this.value.getMinutes(), 0, 0);
		} else if (this.showTime) {
			date.setHours(this.value ? this.value.getHours() : 0, this.value ? this.value.getMinutes() : 0, 0, 0);
		} else {
			date.setHours(0, 0, 0, 0);
		}

		this.setValue(date);
		this.viewDate = new Date(date);
		this.renderCalendar();
		this.syncTimeInputs();
		// Force formatting and commit so the picked date is always serialized
		// into the hidden value immediately after a day click.
		this.commitDisplay(true);
		// Close popup immediately after picking a date.
		this.closePopup();
	}

	updateTimeFromInputs() {
		if (!this.showTime) {
			return;
		}

		if (!this.value) {
			this.value = new Date();
		}

		const hour = Number.parseInt(this.hourInput.value, 10);
		const minute = Number.parseInt(this.minuteInput.value, 10);
		if (!Number.isNaN(hour)) {
			this.value.setHours(Math.min(23, Math.max(0, hour)));
		}
		if (!Number.isNaN(minute)) {
			this.value.setMinutes(Math.min(59, Math.max(0, minute)));
		}

		this.setValue(this.value);
		this.commitDisplay(false);
	}

	syncTimeInputs() {
		if (!this.showTime) {
			return;
		}

		if (!this.value) {
			this.hourInput.value = "";
			this.minuteInput.value = "";
			return;
		}

		this.hourInput.value = String(this.value.getHours());
		this.minuteInput.value = String(this.value.getMinutes()).padStart(2, "0");
	}

	setValue(date) {
		this.value = new Date(date);
		this.value.setSeconds(0, 0);
		this.valueInput.value = this.serialize(this.value);
		this.displayInput.value = this.formatDisplay(this.value);
		this.clearValidation();
	}

	clear() {
		this.value = null;
		this.valueInput.value = "";
		this.displayInput.value = "";
		this.clearValidation();
		this.syncTimeInputs();
		this.closePopup();
	}

	commitDisplay(forceFormat) {
		const text = this.displayInput.value.trim();
		if (!text) {
			if (this.displayInput.required) {
				this.setValidation(this.requiredText, true);
				return false;
			}

			this.clear();
			return true;
		}

		const parsed = this.parseDisplay(text);
		if (!parsed) {
			this.setValidation(this.invalidText, false);
			return false;
		}

		this.setValue(parsed);
		if (forceFormat) {
			this.displayInput.value = this.formatDisplay(parsed);
		}
		this.clearValidation();
		return true;
	}

	setValidation(message, requiredHint) {
		this.displayInput.classList.add("is-invalid");
		this.displayInput.setAttribute("aria-invalid", "true");
		if (requiredHint) {
			this.errorMessage.textContent = "";
			this.setRequiredMessage(message);
			return;
		}

		this.setRequiredMessage("");
		this.errorMessage.textContent = message;
	}

	clearValidation() {
		this.displayInput.classList.remove("is-invalid");
		this.displayInput.removeAttribute("aria-invalid");
		this.errorMessage.textContent = "";
		this.setRequiredMessage("");
	}

	setRequiredMessage(message) {
		if (!this.requiredMessage) {
			return;
		}

		this.requiredMessage.textContent = message;
		this.requiredMessage.classList.toggle("is-visible", Boolean(message));
	}

	parseDisplay(text) {
		const normalizedText = text.trim();
		const isoMatch = normalizedText.match(/^(\d{4})-(\d{2})-(\d{2})(?:[T\s](\d{2}):(\d{2})(?::(\d{2}))?)?$/);
		if (isoMatch) {
			const [, year, month, day, hour = "0", minute = "0", second = "0"] = isoMatch;
			const candidate = new Date(Number(year), Number(month) - 1, Number(day), this.showTime ? Number(hour) : 0, this.showTime ? Number(minute) : 0, this.showTime ? Number(second) : 0);
			return this.isValidCandidate(candidate, Number(year), Number(month), Number(day)) ? candidate : null;
		}

		const timeMatch = this.showTime ? normalizedText.match(/^(.*?)(?:\s+|T)(\d{1,2})[:.](\d{2})$/) : null;
		const datePart = timeMatch ? timeMatch[1].trim() : normalizedText;
		const hour = timeMatch ? Number(timeMatch[2]) : 0;
		const minute = timeMatch ? Number(timeMatch[3]) : 0;
		const numbers = datePart.match(/\d+/g)?.map((value) => Number(value)) ?? [];

		if (numbers.length < 3) {
			return null;
		}

		const orderedValues = new Map([
			[this.order[0], numbers[0]],
			[this.order[1], numbers[1]],
			[this.order[2], numbers[2]]
		]);

		let day = orderedValues.get("day") ?? numbers[0];
		let month = orderedValues.get("month") ?? numbers[1];
		let year = orderedValues.get("year") ?? numbers[2];

		if (year < 100) {
			year += year >= 70 ? 1900 : 2000;
		}

		const candidate = new Date(year, month - 1, day, this.showTime ? hour : 0, this.showTime ? minute : 0, 0, 0);
		return this.isValidCandidate(candidate, year, month, day) ? candidate : null;
	}

	isValidCandidate(candidate, year, month, day) {
		return !Number.isNaN(candidate.getTime())
			&& candidate.getFullYear() === year
			&& candidate.getMonth() === month - 1
			&& candidate.getDate() === day;
	}
}

class TimeControl {
	constructor(root) {
		this.root = root;
		this.displayInput = root.querySelector("[data-ux-time-display]");
		this.valueInput = root.querySelector("[data-ux-time-value]");
		this.toggleButton = root.querySelector("[data-ux-time-toggle]");
		this.popup = root.querySelector("[data-ux-time-popup]");
		this.hourInput = root.querySelector("[data-ux-time-hour]");
		this.minuteInput = root.querySelector("[data-ux-time-minute]");
		this.nowButton = root.querySelector("[data-ux-time-now]");
		this.clearButton = root.querySelector("[data-ux-time-clear]");
		this.doneButton = root.querySelector("[data-ux-time-done]");
		this.presetButtons = Array.from(root.querySelectorAll("[data-ux-time-preset]"));
		this.bindEvents();
		this.syncInputsFromValue();
	}

	bindEvents() {
		this.toggleButton?.addEventListener("click", () => this.togglePopup());
		this.displayInput?.addEventListener("focus", () => this.openPopup());
		this.displayInput?.addEventListener("input", () => this.updateValueFromDisplay(false));
		this.displayInput?.addEventListener("blur", () => {
			window.setTimeout(() => {
				if (this.root.contains(document.activeElement)) {
					return;
				}
				this.updateValueFromDisplay(true);
			}, 0);
		});
		this.displayInput?.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.updateValueFromDisplay(true);
				this.closePopup();
			}
			if (event.key === "Escape") {
				this.closePopup();
			}
		});
		this.hourInput?.addEventListener("input", () => this.updateValueFromPicker(false));
		this.minuteInput?.addEventListener("input", () => this.updateValueFromPicker(false));
		this.hourInput?.addEventListener("change", () => this.updateValueFromPicker(true));
		this.minuteInput?.addEventListener("change", () => this.updateValueFromPicker(true));
		this.hourInput?.addEventListener("keydown", (event) => this.handleNumberKeydown(event, "hour"));
		this.minuteInput?.addEventListener("keydown", (event) => this.handleNumberKeydown(event, "minute"));
		this.nowButton?.addEventListener("click", () => this.setTimeFromDate(new Date()));
		this.clearButton?.addEventListener("click", () => this.clear());
		this.doneButton?.addEventListener("click", () => {
			this.updateValueFromPicker(true);
			this.closePopup();
		});
		this.presetButtons.forEach((button) => {
			button.addEventListener("click", () => this.setValue(button.dataset.uxTimePreset || ""));
		});
		document.addEventListener("click", (event) => {
			if (!(event.target instanceof Node)) {
				return;
			}
			if (!this.root.contains(event.target)) {
				this.closePopup();
			}
		});
		document.addEventListener("keydown", (event) => {
			if (event.key === "Escape") {
				this.closePopup();
			}
		});
	}

	openPopup() {
		if (!this.popup || !this.toggleButton || !this.displayInput) {
			return;
		}

		this.popup.hidden = false;
		this.root.classList.add("ux-time-open");
		this.toggleButton.setAttribute("aria-expanded", "true");
		this.displayInput.setAttribute("aria-expanded", "true");
		this.syncInputsFromValue();
	}

	closePopup() {
		if (!this.popup || !this.toggleButton || !this.displayInput) {
			return;
		}

		this.popup.hidden = true;
		this.root.classList.remove("ux-time-open");
		this.toggleButton.setAttribute("aria-expanded", "false");
		this.displayInput.setAttribute("aria-expanded", "false");
	}

	togglePopup() {
		if (this.popup?.hidden) {
			this.openPopup();
		} else {
			this.closePopup();
		}
	}

	updateValueFromDisplay(format) {
		const parsed = this.parseTime(this.displayInput?.value || "");
		if (!parsed) {
			if (!this.displayInput?.value.trim()) {
				this.clear(false);
			}
			return;
		}

		this.setValue(parsed, format);
	}

	updateValueFromPicker(format) {
		const hour = Number.parseInt(this.hourInput?.value || "", 10);
		const minute = Number.parseInt(this.minuteInput?.value || "", 10);
		if (Number.isNaN(hour) || Number.isNaN(minute)) {
			return;
		}

		const wrappedHour = this.wrapNumber(hour, 23);
		const wrappedMinute = this.wrapNumber(minute, 59);
		this.setValue(`${String(wrappedHour).padStart(2, "0")}:${String(wrappedMinute).padStart(2, "0")}`, format);
	}

	handleNumberKeydown(event, unit) {
		if (event.key !== "ArrowUp" && event.key !== "ArrowDown") {
			return;
		}

		event.preventDefault();
		const input = unit === "hour" ? this.hourInput : this.minuteInput;
		const max = unit === "hour" ? 23 : 59;
		const fallback = event.key === "ArrowUp" ? -1 : 0;
		const currentValue = Number.parseInt(input?.value || "", 10);
		const current = Number.isNaN(currentValue) ? fallback : currentValue;
		const next = this.wrapNumber(current + (event.key === "ArrowUp" ? 1 : -1), max);

		if (input) {
			input.value = String(next).padStart(2, "0");
		}

		this.updateValueFromPicker(true);
	}

	wrapNumber(value, max) {
		if (value > max) {
			return 0;
		}

		if (value < 0) {
			return max;
		}

		return value;
	}

	setTimeFromDate(date) {
		this.setValue(`${String(date.getHours()).padStart(2, "0")}:${String(date.getMinutes()).padStart(2, "0")}`, true);
	}

	setValue(value, format = true) {
		const parsed = this.parseTime(value);
		if (!parsed || !this.displayInput || !this.valueInput) {
			return;
		}

		this.valueInput.value = parsed;
		this.displayInput.value = format ? parsed : value;
		this.displayInput.classList.remove("is-invalid");
		this.syncInputsFromValue();
	}

	clear(closePopup = true) {
		if (this.displayInput) {
			this.displayInput.value = "";
		}
		if (this.valueInput) {
			this.valueInput.value = "";
		}
		if (this.hourInput) {
			this.hourInput.value = "";
		}
		if (this.minuteInput) {
			this.minuteInput.value = "";
		}
		if (closePopup) {
			this.closePopup();
		}
	}

	syncInputsFromValue() {
		const parsed = this.parseTime(this.valueInput?.value || this.displayInput?.value || "");
		if (!parsed || !this.hourInput || !this.minuteInput) {
			return;
		}

		const [hour, minute] = parsed.split(":");
		this.hourInput.value = hour;
		this.minuteInput.value = minute;
	}

	parseTime(value) {
		const text = value.trim();
		const match = text.match(/^(\d{1,2})(?::|\.|\s)?(\d{2})$/);
		if (!match) {
			return null;
		}

		const hour = Number.parseInt(match[1], 10);
		const minute = Number.parseInt(match[2], 10);
		if (Number.isNaN(hour) || Number.isNaN(minute) || hour > 23 || minute > 59) {
			return null;
		}

		return `${String(hour).padStart(2, "0")}:${String(minute).padStart(2, "0")}`;
	}
}

class PhoneNumberControl {
	constructor(root) {
		this.root = root;
		this.localInput = root.querySelector("[data-ux-phone-local-input]");
		this.bindEvents();
		this.filterValue();
	}

	bindEvents() {
		this.localInput?.addEventListener("beforeinput", (event) => this.preventInvalidInput(event));
		this.localInput?.addEventListener("keydown", (event) => this.preventInvalidKeydown(event));
		this.localInput?.addEventListener("input", () => this.filterValue());
		this.localInput?.addEventListener("paste", () => window.setTimeout(() => this.filterValue(), 0));
	}

	preventInvalidInput(event) {
		if (event.inputType !== "insertText" || typeof event.data !== "string") {
			return;
		}

		if (/[^\d]/.test(event.data)) {
			event.preventDefault();
		}
	}

	preventInvalidKeydown(event) {
		if (event.ctrlKey || event.metaKey || event.altKey) {
			return;
		}

		if (event.key.length === 1 && /[^\d]/.test(event.key)) {
			event.preventDefault();
		}
	}

	filterValue() {
		if (!this.localInput) {
			return;
		}

		const filtered = this.localInput.value.replace(/\D+/g, "").slice(0, 9);
		if (this.localInput.value !== filtered) {
			this.localInput.value = filtered;
		}
	}
}

class PhoneCountryControl {
	constructor(root) {
		this.root = root;
		this.toggleButton = root.querySelector("[data-ux-phone-country-toggle]");
		this.panel = root.querySelector("[data-ux-phone-country-panel]");
		this.valueInput = root.querySelector("[data-ux-phone-country-value]");
		this.label = root.querySelector("[data-ux-phone-country-label]");
		this.optionButtons = Array.from(root.querySelectorAll("[data-ux-phone-country-option]"));
		this.bindEvents();
	}

	bindEvents() {
		this.toggleButton?.addEventListener("click", () => this.toggle());
		this.optionButtons.forEach((button) => {
			button.addEventListener("click", () => {
				this.select(button.dataset.dialCode || "", button.dataset.displayText || button.textContent || "");
			});
		});

		document.addEventListener("click", (event) => {
			const target = event.target;
			if (!(target instanceof Node)) {
				return;
			}

			if (!this.root.contains(target)) {
				this.close();
			}
		});

		document.addEventListener("keydown", (event) => {
			if (event.key === "Escape") {
				this.close();
			}
		});
	}

	toggle() {
		if (this.panel?.hidden) {
			this.open();
		} else {
			this.close(true);
		}
	}

	open() {
		if (!this.panel || !this.toggleButton) {
			return;
		}

		this.panel.hidden = false;
		this.root.classList.add("is-open");
		this.toggleButton.setAttribute("aria-expanded", "true");
		const selectedOption = this.optionButtons.find((button) => button.classList.contains("is-selected"));
		(selectedOption || this.optionButtons[0] || this.toggleButton).focus();
	}

	close(returnFocus = false) {
		if (!this.panel || !this.toggleButton) {
			return;
		}

		this.panel.hidden = true;
		this.root.classList.remove("is-open");
		this.toggleButton.setAttribute("aria-expanded", "false");
		if (returnFocus) {
			this.toggleButton.focus();
		}
	}

	select(dialCode, displayText) {
		if (this.valueInput) {
			this.valueInput.value = dialCode;
		}

		if (this.label) {
			this.label.textContent = displayText;
		}

		this.optionButtons.forEach((button) => {
			button.classList.toggle("is-selected", button.dataset.dialCode === dialCode);
		});

		this.close(true);
	}
}

class AutocompleteDropdown {
	constructor(root) {
		this.root = root;
		this.searchUrl = root.dataset.uxAutocompleteUrl || "";
		this.valueInput = root.querySelector("[data-ux-autocomplete-value]");
		this.textInput = root.querySelector("[data-ux-autocomplete-text]");
		this.list = root.querySelector("[data-ux-autocomplete-list]");
		this.error = root.querySelector("[data-ux-autocomplete-error]");
		this.requiredText = root.dataset.uxAutocompleteRequiredMessage || "This field is required.";
		this.selectionText = root.dataset.uxAutocompleteSelectionMessage || "Choose an item from the list.";
		this.abortController = null;
		this.searchTimer = null;
		this.activeIndex = -1;
		this.options = [];
		this.bindEvents();
	}

	bindEvents() {
		this.textInput?.addEventListener("input", () => {
			if (this.valueInput) {
				this.valueInput.value = "";
			}
			this.clearValidation();
			this.queueSearch();
		});
		this.textInput?.addEventListener("focus", () => this.queueSearch(true));
		this.textInput?.addEventListener("blur", () => this.validateSelection());
		this.textInput?.addEventListener("keydown", (event) => this.handleKeydown(event));
		this.list?.addEventListener("mousedown", (event) => this.handleListMousedown(event));
		document.addEventListener("click", (event) => {
			if (!(event.target instanceof Node)) {
				return;
			}
			if (!this.root.contains(event.target)) {
				this.closeList();
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}
		if (immediate) {
			this.runSearch();
			return;
		}
		this.searchTimer = window.setTimeout(() => this.runSearch(), 220);
	}

	async runSearch() {
		if (!this.searchUrl || !this.textInput || !this.list) {
			return;
		}

		const query = this.textInput.value.trim();
		if (this.abortController) {
			this.abortController.abort();
		}
		this.abortController = new AbortController();

		try {
			const requestUrl = new URL(this.searchUrl, window.location.origin);
			requestUrl.searchParams.set("query", query);
			const response = await fetch(requestUrl.toString(), {
				headers: { "X-Requested-With": "XMLHttpRequest" },
				signal: this.abortController.signal
			});
			if (!response.ok) {
				return;
			}

			const options = await response.json();
			if (!Array.isArray(options)) {
				return;
			}

			this.options = options;
			this.activeIndex = -1;
			this.renderOptions();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		}
	}

	renderOptions() {
		if (!this.list) {
			return;
		}

		if (this.options.length === 0) {
			this.list.innerHTML = "";
			this.closeList();
			return;
		}

		this.list.innerHTML = this.options
			.map((item, index) => `<button type="button" class="ux-autocomplete-option" data-ux-autocomplete-option data-index="${index}" role="option">${this.escapeHtml(item.text ?? "")}</button>`)
			.join("");
		this.list.hidden = false;
	}

	handleListMousedown(event) {
		const target = event.target;
		if (!(target instanceof Element)) {
			return;
		}

		const button = target.closest("[data-ux-autocomplete-option]");
		if (!button) {
			return;
		}

		event.preventDefault();
		const index = Number(button.getAttribute("data-index"));
		if (Number.isNaN(index) || !this.options[index]) {
			return;
		}
		this.selectOption(this.options[index]);
	}

	handleKeydown(event) {
		if (!this.list || this.list.hidden) {
			if (event.key === "ArrowDown") {
				this.queueSearch(true);
			}
			return;
		}

		if (event.key === "ArrowDown") {
			event.preventDefault();
			this.activeIndex = Math.min(this.activeIndex + 1, this.options.length - 1);
			this.refreshActiveOption();
			return;
		}

		if (event.key === "ArrowUp") {
			event.preventDefault();
			this.activeIndex = Math.max(this.activeIndex - 1, 0);
			this.refreshActiveOption();
			return;
		}

		if (event.key === "Enter") {
			event.preventDefault();
			if (this.activeIndex >= 0 && this.options[this.activeIndex]) {
				this.selectOption(this.options[this.activeIndex]);
			}
			return;
		}

		if (event.key === "Escape") {
			event.preventDefault();
			this.closeList();
		}
	}

	refreshActiveOption() {
		if (!this.list) {
			return;
		}
		const optionButtons = this.list.querySelectorAll("[data-ux-autocomplete-option]");
		optionButtons.forEach((button, index) => {
			button.classList.toggle("is-active", index === this.activeIndex);
		});
	}

	selectOption(option) {
		if (!this.valueInput || !this.textInput) {
			return;
		}

		this.valueInput.value = String(option.id ?? "");
		this.textInput.value = String(option.text ?? "");
		this.clearValidation();
		this.closeList();
	}

	validateSelection() {
		if (!this.textInput || !this.valueInput) {
			return true;
		}

		const visibleValue = this.textInput.value.trim();
		const selectedValue = this.valueInput.value.trim();
		if (visibleValue.length === 0) {
			if (this.textInput.required) {
				this.setValidation(this.requiredText);
				return false;
			}

			this.clearValidation();
			return true;
		}

		if (selectedValue.length === 0) {
			this.setValidation(this.selectionText);
			return false;
		}

		this.clearValidation();
		return true;
	}

	setValidation(message) {
		if (!this.textInput) {
			return;
		}

		this.textInput.classList.add("is-invalid");
		this.textInput.setAttribute("aria-invalid", "true");
		if (this.error) {
			this.error.classList.remove("d-none");
			this.error.textContent = message;
		}
	}

	clearValidation() {
		if (!this.textInput) {
			return;
		}

		this.textInput.classList.remove("is-invalid");
		this.textInput.removeAttribute("aria-invalid");
		if (this.error) {
			this.error.classList.add("d-none");
			this.error.textContent = "";
		}
	}

	closeList() {
		if (!this.list) {
			return;
		}
		this.list.hidden = true;
		this.list.innerHTML = "";
		this.activeIndex = -1;
	}

	escapeHtml(value) {
		return value
			.replaceAll("&", "&amp;")
			.replaceAll("<", "&lt;")
			.replaceAll(">", "&gt;");
	}
}

class CustomerDirectorySearch {
	constructor(input) {
		this.input = input;
		this.tableBody = document.querySelector("[data-ux-customer-table-body]");
		this.pendingRequest = null;
		this.searchTimer = null;
		this.bindEvents();
	}

	bindEvents() {
		this.input.addEventListener("input", () => this.queueSearch());
		this.input.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.queueSearch(true);
				return;
			}

			if (event.key === "Escape") {
				event.preventDefault();
				this.input.value = "";
				this.queueSearch(true);
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}

		if (immediate) {
			this.runSearch();
			return;
		}

		this.searchTimer = window.setTimeout(() => this.runSearch(), 250);
	}

	async runSearch() {
		if (!this.tableBody) {
			return;
		}

		const query = this.input.value.trim();

		if (this.pendingRequest) {
			this.pendingRequest.abort();
		}

		const controller = new AbortController();
		this.pendingRequest = controller;
		const baseUrl = `${window.location.origin}${window.location.pathname.replace(/\/?$/, "/")}`;
		const requestUrl = new URL("pretraga", baseUrl);
		requestUrl.searchParams.set("query", query);

		try {
			const response = await fetch(requestUrl.toString(), {
				headers: {
					"X-Requested-With": "XMLHttpRequest"
				},
				signal: controller.signal
			});

			if (!response.ok) {
				return;
			}

			this.tableBody.innerHTML = await response.text();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		} finally {
			if (this.pendingRequest === controller) {
				this.pendingRequest = null;
			}
		}
	}
}

class LocationDirectorySearch {
	constructor(input) {
		this.input = input;
		this.tableBody = document.querySelector("[data-ux-location-table-body]");
		this.pendingRequest = null;
		this.searchTimer = null;
		this.bindEvents();
	}

	bindEvents() {
		this.input.addEventListener("input", () => this.queueSearch());
		this.input.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.queueSearch(true);
				return;
			}

			if (event.key === "Escape") {
				event.preventDefault();
				this.input.value = "";
				this.queueSearch(true);
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}

		if (immediate) {
			this.runSearch();
			return;
		}

		this.searchTimer = window.setTimeout(() => this.runSearch(), 250);
	}

	async runSearch() {
		if (!this.tableBody) {
			return;
		}

		const query = this.input.value.trim();

		if (this.pendingRequest) {
			this.pendingRequest.abort();
		}

		const controller = new AbortController();
		this.pendingRequest = controller;
		const baseUrl = `${window.location.origin}${window.location.pathname.replace(/\/?$/, "/")}`;
		const requestUrl = new URL("pretraga", baseUrl);
		requestUrl.searchParams.set("query", query);

		try {
			const response = await fetch(requestUrl.toString(), {
				headers: {
					"X-Requested-With": "XMLHttpRequest"
				},
				signal: controller.signal
			});

			if (!response.ok) {
				return;
			}

			this.tableBody.innerHTML = await response.text();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		} finally {
			if (this.pendingRequest === controller) {
				this.pendingRequest = null;
			}
		}
	}
}

class AgentDirectorySearch {
	constructor(input) {
		this.input = input;
		this.tableBody = document.querySelector("[data-ux-agent-table-body]");
		this.pendingRequest = null;
		this.searchTimer = null;
		this.bindEvents();
	}

	bindEvents() {
		this.input.addEventListener("input", () => this.queueSearch());
		this.input.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.queueSearch(true);
				return;
			}

			if (event.key === "Escape") {
				event.preventDefault();
				this.input.value = "";
				this.queueSearch(true);
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}

		if (immediate) {
			this.runSearch();
			return;
		}

		this.searchTimer = window.setTimeout(() => this.runSearch(), 250);
	}

	async runSearch() {
		if (!this.tableBody) {
			return;
		}

		const query = this.input.value.trim();

		if (this.pendingRequest) {
			this.pendingRequest.abort();
		}

		const controller = new AbortController();
		this.pendingRequest = controller;
		const baseUrl = `${window.location.origin}${window.location.pathname.replace(/\/?$/, "/")}`;
		const requestUrl = new URL("pretraga", baseUrl);
		requestUrl.searchParams.set("query", query);

		try {
			const response = await fetch(requestUrl.toString(), {
				headers: {
					"X-Requested-With": "XMLHttpRequest"
				},
				signal: controller.signal
			});

			if (!response.ok) {
				return;
			}

			this.tableBody.innerHTML = await response.text();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		} finally {
			if (this.pendingRequest === controller) {
				this.pendingRequest = null;
			}
		}
	}
}

class BookingDirectorySearch {
	constructor(input) {
		this.input = input;
		this.tableBody = document.querySelector("[data-ux-booking-table-body]");
		this.pendingRequest = null;
		this.searchTimer = null;
		this.bindEvents();
	}

	bindEvents() {
		this.input.addEventListener("input", () => this.queueSearch());
		this.input.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.queueSearch(true);
				return;
			}

			if (event.key === "Escape") {
				event.preventDefault();
				this.input.value = "";
				this.queueSearch(true);
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}

		if (immediate) {
			this.runSearch();
			return;
		}

		this.searchTimer = window.setTimeout(() => this.runSearch(), 250);
	}

	async runSearch() {
		if (!this.tableBody) {
			return;
		}

		const query = this.input.value.trim();

		if (this.pendingRequest) {
			this.pendingRequest.abort();
		}

		const controller = new AbortController();
		this.pendingRequest = controller;
		const baseUrl = `${window.location.origin}${window.location.pathname.replace(/\/?$/, "/")}`;
		const requestUrl = new URL("pretraga", baseUrl);
		requestUrl.searchParams.set("query", query);

		try {
			const response = await fetch(requestUrl.toString(), {
				headers: {
					"X-Requested-With": "XMLHttpRequest"
				},
				signal: controller.signal
			});

			if (!response.ok) {
				return;
			}

			this.tableBody.innerHTML = await response.text();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		} finally {
			if (this.pendingRequest === controller) {
				this.pendingRequest = null;
			}
		}
	}
}

class TicketDirectorySearch {
	constructor(input) {
		this.input = input;
		this.tableBody = document.querySelector("[data-ux-ticket-table-body]");
		this.pendingRequest = null;
		this.searchTimer = null;
		this.bindEvents();
	}

	bindEvents() {
		this.input.addEventListener("input", () => this.queueSearch());
		this.input.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.queueSearch(true);
				return;
			}

			if (event.key === "Escape") {
				event.preventDefault();
				this.input.value = "";
				this.queueSearch(true);
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}

		if (immediate) {
			this.runSearch();
			return;
		}

		this.searchTimer = window.setTimeout(() => this.runSearch(), 250);
	}

	async runSearch() {
		if (!this.tableBody) {
			return;
		}

		const query = this.input.value.trim();

		if (this.pendingRequest) {
			this.pendingRequest.abort();
		}

		const controller = new AbortController();
		this.pendingRequest = controller;
		const baseUrl = `${window.location.origin}${window.location.pathname.replace(/\/?$/, "/")}`;
		const requestUrl = new URL("pretraga", baseUrl);
		requestUrl.searchParams.set("query", query);

		try {
			const response = await fetch(requestUrl.toString(), {
				headers: {
					"X-Requested-With": "XMLHttpRequest"
				},
				signal: controller.signal
			});

			if (!response.ok) {
				return;
			}

			this.tableBody.innerHTML = await response.text();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		} finally {
			if (this.pendingRequest === controller) {
				this.pendingRequest = null;
			}
		}
	}
}

class VehicleDirectorySearch {
	constructor(input) {
		this.input = input;
		this.tableBody = document.querySelector("[data-ux-vehicle-table-body]");
		this.pendingRequest = null;
		this.searchTimer = null;
		this.bindEvents();
	}

	bindEvents() {
		this.input.addEventListener("input", () => this.queueSearch());
		this.input.addEventListener("keydown", (event) => {
			if (event.key === "Enter") {
				event.preventDefault();
				this.queueSearch(true);
				return;
			}

			if (event.key === "Escape") {
				event.preventDefault();
				this.input.value = "";
				this.queueSearch(true);
			}
		});
	}

	queueSearch(immediate = false) {
		if (this.searchTimer) {
			window.clearTimeout(this.searchTimer);
		}

		if (immediate) {
			this.runSearch();
			return;
		}

		this.searchTimer = window.setTimeout(() => this.runSearch(), 250);
	}

	async runSearch() {
		if (!this.tableBody) {
			return;
		}

		const query = this.input.value.trim();

		if (this.pendingRequest) {
			this.pendingRequest.abort();
		}

		const controller = new AbortController();
		this.pendingRequest = controller;
		const baseUrl = `${window.location.origin}${window.location.pathname.replace(/\/?$/, "/")}`;
		const requestUrl = new URL("pretraga", baseUrl);
		requestUrl.searchParams.set("query", query);

		try {
			const response = await fetch(requestUrl.toString(), {
				headers: {
					"X-Requested-With": "XMLHttpRequest"
				},
				signal: controller.signal
			});

			if (!response.ok) {
				return;
			}

			this.tableBody.innerHTML = await response.text();
		} catch (error) {
			if (error?.name !== "AbortError") {
				return;
			}
		} finally {
			if (this.pendingRequest === controller) {
				this.pendingRequest = null;
			}
		}
	}
}

document.querySelectorAll("[data-ux-datetime-control]").forEach((root) => {
	new DateTimeControl(root);
});

document.querySelectorAll("[data-ux-time-control]").forEach((root) => {
	new TimeControl(root);
});

document.querySelectorAll("[data-ux-phone-control]").forEach((root) => {
	new PhoneNumberControl(root);
});

document.querySelectorAll("[data-ux-phone-country-control]").forEach((root) => {
	new PhoneCountryControl(root);
});

document.querySelectorAll("[data-ux-autocomplete-control]").forEach((root) => {
	new AutocompleteDropdown(root);
});

document.querySelectorAll("[data-ux-customer-search]").forEach((input) => {
	new CustomerDirectorySearch(input);
});

document.querySelectorAll("[data-ux-location-search]").forEach((input) => {
	new LocationDirectorySearch(input);
});

document.querySelectorAll("[data-ux-agent-search]").forEach((input) => {
	new AgentDirectorySearch(input);
});

document.querySelectorAll("[data-ux-booking-search]").forEach((input) => {
	new BookingDirectorySearch(input);
});

document.querySelectorAll("[data-ux-ticket-search]").forEach((input) => {
	new TicketDirectorySearch(input);
});

document.querySelectorAll("[data-ux-vehicle-search]").forEach((input) => {
	new VehicleDirectorySearch(input);
});

// Bulk selection and delete UI handling (generic)
document.querySelectorAll("form[data-ux-bulk-form]").forEach((form) => {
	const selectAll = form.querySelector('[data-ux-select-all]');
	const deleteBtnId = form.getAttribute('id');
	const deleteBtn = deleteBtnId
		? document.querySelector(`[data-ux-bulk-delete][form="${deleteBtnId}"]`) ?? form.querySelector('[data-ux-bulk-delete]')
		: form.querySelector('[data-ux-bulk-delete]');

	function getCheckboxes() {
		return Array.from(form.querySelectorAll('input[type="checkbox"][name="ids"]'));
	}

	function updateState() {
		const checkboxes = getCheckboxes();
		const selectedCount = checkboxes.filter((checkbox) => checkbox.checked).length;
		if (deleteBtn) {
			deleteBtn.disabled = selectedCount === 0;
		}
		if (selectAll) {
			selectAll.checked = checkboxes.length > 0 && selectedCount === checkboxes.length;
			selectAll.indeterminate = selectedCount > 0 && selectedCount < checkboxes.length;
		}
	}

	if (selectAll) {
		selectAll.addEventListener('change', (event) => {
			const checked = event.target.checked;
			getCheckboxes().forEach((checkbox) => {
				checkbox.checked = checked;
			});
			updateState();
		});
	}

	form.addEventListener('change', (event) => {
		const target = event.target;
		if (!(target instanceof HTMLInputElement) || target.name !== 'ids') {
			return;
		}

		updateState();
	});

	updateState();
});

function closeOpenEllipsisMenus(exceptMenu = null) {
	document.querySelectorAll('.ux-ellipsis-menu.open').forEach((menu) => {
		if (menu === exceptMenu) {
			return;
		}

		menu.classList.remove('open');
		menu.querySelector('.ux-ellipsis-btn')?.setAttribute('aria-expanded', 'false');
	});
}

document.addEventListener('click', (event) => {
	const target = event.target;
	if (!(target instanceof Element)) {
		closeOpenEllipsisMenus();
		return;
	}

	const button = target.closest('.ux-ellipsis-btn');
	if (button) {
		event.stopPropagation();
		const menu = button.closest('.ux-ellipsis-menu');
		if (!menu) {
			return;
		}

		const open = !menu.classList.contains('open');
		closeOpenEllipsisMenus(menu);
		menu.classList.toggle('open', open);
		button.setAttribute('aria-expanded', open ? 'true' : 'false');
		return;
	}

	if (!target.closest('.ux-ellipsis-menu')) {
		closeOpenEllipsisMenus();
	}
});

// Compact sort toggle handlers
document.querySelectorAll('.ux-sort-toggle').forEach(btn => {
	btn.addEventListener('click', (e) => {
		const key = btn.getAttribute('data-sort-key');
		if (!key) return;

		const url = new URL(window.location.href);
		const current = url.searchParams.get('sort') || '';
		// determine current direction for this key
		const asc = `${key}_asc`;
		const desc = `${key}_desc`;

		if (current === asc) {
			url.searchParams.set('sort', desc);
			btn.classList.add('active');
		} else if (current === desc) {
			url.searchParams.set('sort', asc);
			btn.classList.add('active');
		} else {
			// default to asc
			url.searchParams.set('sort', asc);
			btn.classList.add('active');
		}

		// update UI: clear other toggles
		document.querySelectorAll('.ux-sort-toggle').forEach(b => { if (b !== btn) b.classList.remove('active'); });

		// navigate to new url
		window.location.href = url.toString();
	});
});

// initialize active sort toggle based on current query param
(function initSortToggles() {
	const url = new URL(window.location.href);
	const current = url.searchParams.get('sort') || '';
	if (!current) return;
	document.querySelectorAll('.ux-sort-toggle').forEach(btn => {
		const key = btn.getAttribute('data-sort-key');
		if (!key) return;
		if (current.startsWith(key + '_')) btn.classList.add('active');
	});
})();
