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
