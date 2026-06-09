(() => {
    const state = {
        employeeId: "",
        group: "",
        dateFrom: "",
        dateTo: "",
        pageIndex: 1,
        pageSize: 10,
        totalCount: 0,
        totalPages: 1,
        rows: []
    };

    const refs = {};

    document.addEventListener("DOMContentLoaded", init);

    function init() {
        cacheElements();
        bindEvents();
        fetchAndRender();
    }

    function cacheElements() {
        refs.searchEmployeeId = document.getElementById("searchEmployeeId");
        refs.searchGroup = document.getElementById("searchGroup");
        refs.dateFrom = document.getElementById("dateFrom");
        refs.dateTo = document.getElementById("dateTo");
        refs.searchBtn = document.getElementById("searchBtn");
        refs.resetBtn = document.getElementById("resetBtn");
        refs.exportBtn = document.getElementById("exportBtn");
        refs.pageSizeSelect = document.getElementById("pageSizeSelect");
        refs.paginationControls = document.getElementById("paginationControls");
        refs.pageNumbers = document.getElementById("pageNumbers");
        refs.showingCount = document.getElementById("showingCount");
        refs.totalCount = document.getElementById("totalCount");
        refs.reviewsTableBody = document.getElementById("reviewsTableBody");

        refs.detailModal = document.getElementById("detailModal");
        refs.modalBody = document.getElementById("modalBody");
        refs.modalClose = document.querySelector(".modal-close");
    }

    function bindEvents() {
        refs.searchBtn?.addEventListener("click", async () => {
            state.pageIndex = 1;
            syncFilterState();
            await fetchAndRender(true);
        });

        refs.resetBtn?.addEventListener("click", async () => {
            resetFilters();
            await fetchAndRender();
            showNotify("success", "Thành công", "Đã reset bộ lọc.");
        });

        refs.exportBtn?.addEventListener("click", exportExcel);

        refs.pageSizeSelect?.addEventListener("change", async () => {
            state.pageSize = Number(refs.pageSizeSelect.value) || 10;
            state.pageIndex = 1;
            await fetchAndRender();
        });

        refs.paginationControls?.addEventListener("click", async (e) => {
            const btn = e.target.closest("button.page-btn");
            if (!btn) {
                return;
            }

            if (btn.dataset.page === "prev" && state.pageIndex > 1) {
                state.pageIndex -= 1;
                await fetchAndRender(false);
                return;
            }

            if (btn.dataset.page === "next" && state.pageIndex < state.totalPages) {
                state.pageIndex += 1;
                await fetchAndRender(false);
                return;
            }

            const page = Number(btn.dataset.page);
            if (!Number.isNaN(page) && page >= 1 && page <= state.totalPages) {
                state.pageIndex = page;
                await fetchAndRender(false);
            }
        });

        refs.reviewsTableBody?.addEventListener("click", (e) => {
            const detailBtn = e.target.closest(".btn-detail");
            if (!detailBtn) {
                return;
            }
            const index = Number(detailBtn.dataset.index);
            if (Number.isNaN(index) || !state.rows[index]) {
                return;
            }
            openDetailModal(state.rows[index]);
        });

        refs.modalClose?.addEventListener("click", closeDetailModal);
        refs.detailModal?.addEventListener("click", (e) => {
            if (e.target === refs.detailModal) {
                closeDetailModal();
            }
        });
    }

    function syncFilterState() {
        state.employeeId = (refs.searchEmployeeId?.value || "").trim();
        state.group = (refs.searchGroup?.value || "").trim();
        state.dateFrom = refs.dateFrom?.value || "";
        state.dateTo = refs.dateTo?.value || "";
        state.pageSize = Number(refs.pageSizeSelect?.value) || 10;
    }

    function resetFilters() {
        if (refs.searchEmployeeId) refs.searchEmployeeId.value = "";
        if (refs.searchGroup) refs.searchGroup.value = "";
        if (refs.dateFrom) refs.dateFrom.value = "";
        if (refs.dateTo) refs.dateTo.value = "";
        if (refs.pageSizeSelect) refs.pageSizeSelect.value = "10";

        state.employeeId = "";
        state.group = "";
        state.dateFrom = "";
        state.dateTo = "";
        state.pageIndex = 1;
        state.pageSize = 10;
    }

    async function fetchAndRender(showSuccessMessage = false) {
        syncFilterState();

        if (state.dateFrom && state.dateTo && state.dateFrom > state.dateTo) {
            showNotify("error", "Lỗi", "Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            return;
        }

        renderLoading();

        try {
            const [pagedRows, allRows] = await Promise.all([
                fetchReviews({
                    employeeId: state.employeeId,
                    group: state.group,
                    dateFrom: state.dateFrom,
                    dateTo: state.dateTo,
                    pageIndex: state.pageIndex,
                    pageSize: state.pageSize
                }),
                fetchReviews({
                    employeeId: state.employeeId,
                    group: state.group,
                    dateFrom: state.dateFrom,
                    dateTo: state.dateTo,
                    pageIndex: null,
                    pageSize: null
                })
            ]);

            state.rows = pagedRows;
            state.totalCount = allRows.length;
            state.totalPages = Math.max(1, Math.ceil(state.totalCount / state.pageSize));

            if (state.pageIndex > state.totalPages) {
                state.pageIndex = state.totalPages;
                await fetchAndRender();
                return;
            }

            renderRows(state.rows);
            renderPagination();
            updateInfo();

            if (showSuccessMessage) {
                showNotify("success", "Thành công", "Tìm kiếm dữ liệu thành công.");
            }
        } catch (err) {
            renderErrorRow(getErrorMessage(err));
            renderPagination();
            updateInfo();
            showNotify("error", "Lỗi", getErrorMessage(err));
        }
    }

    async function fetchReviews(payload) {
        const response = await fetch("/Review/SearchReviews", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                employeeId: payload.employeeId || null,
                group: payload.group || null,
                dateFrom: payload.dateFrom || null,
                dateTo: payload.dateTo || null,
                pageIndex: payload.pageIndex,
                pageSize: payload.pageSize
            })
        });

        if (!response.ok) {
            const text = await response.text();
            throw new Error(text || "Không thể tải dữ liệu đánh giá.");
        }

        const data = await response.json();
        return Array.isArray(data) ? data : [];
    }

    function renderLoading() {
        refs.reviewsTableBody.innerHTML = `
            <tr class="loading-row">
                <td colspan="32" style="text-align: center; padding: 60px;">
                    <div class="loading-spinner"></div>
                    <p>Đang tải dữ liệu...</p>
                </td>
            </tr>`;
    }

    function renderErrorRow(message) {
        refs.reviewsTableBody.innerHTML = `
            <tr>
                <td colspan="32" style="text-align: center; padding: 40px; color: #dc2626; font-weight: 600;">${escapeHtml(message)}</td>
            </tr>`;
    }

    function renderRows(rows) {
        if (!rows.length) {
            refs.reviewsTableBody.innerHTML = `
                <tr>
                    <td colspan="32" style="text-align: center; padding: 40px; color: #64748b;">Không có dữ liệu.</td>
                </tr>`;
            return;
        }

        refs.reviewsTableBody.innerHTML = rows.map((item, index) => {
            const stt = ((state.pageIndex - 1) * state.pageSize) + index + 1;

            return `<tr>
                <td>${stt}</td>
                <td>${escapeHtml(item.employee_code)}</td>
                <td>${escapeHtml(item.department)}</td>
                <td>${formatDate(item.created_at)}</td>

                <td>${escapeHtml(item.g1_good_point)}</td>
                <td>${safeValue(item.g1_good_score)}</td>
                <td>${escapeHtml(item.g1_improve_point)}</td>
                <td>${safeValue(item.g1_improve_score)}</td>
                <td>${escapeHtml(item.g1_example)}</td>

                <td>${escapeHtml(item.g2_good_point)}</td>
                <td>${safeValue(item.g2_good_score)}</td>
                <td>${escapeHtml(item.g2_improve_point)}</td>
                <td>${safeValue(item.g2_improve_score)}</td>
                <td>${escapeHtml(item.g2_example)}</td>

                <td>${escapeHtml(item.g3_good_point)}</td>
                <td>${safeValue(item.g3_good_score)}</td>
                <td>${escapeHtml(item.g3_improve_point)}</td>
                <td>${safeValue(item.g3_improve_score)}</td>
                <td>${escapeHtml(item.g3_example)}</td>

                <td>${escapeHtml(item.g4_good_point)}</td>
                <td>${safeValue(item.g4_good_score)}</td>
                <td>${escapeHtml(item.g4_improve_point)}</td>
                <td>${safeValue(item.g4_improve_score)}</td>
                <td>${escapeHtml(item.g4_example)}</td>

                <td>${escapeHtml(item.g5_good_point)}</td>
                <td>${safeValue(item.g5_good_score)}</td>
                <td>${escapeHtml(item.g5_improve_point)}</td>
                <td>${safeValue(item.g5_improve_score)}</td>
                <td>${escapeHtml(item.g5_example)}</td>

                <td>${escapeHtml(item.improvement_proposal)}</td>
                <td>${formatScore(item.total_score)}</td>
                <td><button type="button" class="btn-detail" data-index="${index}">Chi tiết</button></td>
            </tr>`;
        }).join("");
    }

    function renderPagination() {
        const prevBtn = refs.paginationControls?.querySelector('[data-page="prev"]');
        const nextBtn = refs.paginationControls?.querySelector('[data-page="next"]');

        if (prevBtn) {
            prevBtn.disabled = state.pageIndex <= 1;
        }

        if (nextBtn) {
            nextBtn.disabled = state.pageIndex >= state.totalPages;
        }

        const pages = buildPages(state.pageIndex, state.totalPages);
        refs.pageNumbers.innerHTML = pages.map((page) => {
            if (page === "...") {
                return '<span class="page-dot">...</span>';
            }
            const activeClass = page === state.pageIndex ? "active" : "";
            return `<button type="button" class="page-btn ${activeClass}" data-page="${page}">${page}</button>`;
        }).join("");
    }

    function buildPages(currentPage, totalPages) {
        if (totalPages <= 7) {
            return Array.from({ length: totalPages }, (_, i) => i + 1);
        }

        const pages = [1];
        const start = Math.max(2, currentPage - 1);
        const end = Math.min(totalPages - 1, currentPage + 1);

        if (start > 2) {
            pages.push("...");
        }

        for (let i = start; i <= end; i += 1) {
            pages.push(i);
        }

        if (end < totalPages - 1) {
            pages.push("...");
        }

        pages.push(totalPages);
        return pages;
    }

    function updateInfo() {
        const showing = state.rows.length;
        refs.showingCount.textContent = String(showing);
        refs.totalCount.textContent = String(state.totalCount);
    }

    function openDetailModal(item) {
        refs.modalBody.innerHTML = `
            <div><strong>Mã nhân viên:</strong> ${escapeHtml(item.employee_code)}</div>
            <div><strong>Phòng ban:</strong> ${escapeHtml(item.department)}</div>
            <div><strong>Ngày đánh giá:</strong> ${formatDate(item.created_at)}</div>
            <hr/>
            <div><strong>Đề xuất cải tiến:</strong><br/>${escapeHtml(item.improvement_proposal)}</div>
            <div style="margin-top: 8px;"><strong>Điểm trung bình:</strong> ${formatScore(item.total_score)}</div>
        `;
        refs.detailModal.style.display = "block";
    }

    function closeDetailModal() {
        refs.detailModal.style.display = "none";
    }

    async function exportExcel() {
        syncFilterState();

        try {
            const response = await fetch("/Review/ExportReviewsToExcel", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    employeeId: state.employeeId || null,
                    group: state.group || null,
                    dateFrom: state.dateFrom || null,
                    dateTo: state.dateTo || null,
                    pageIndex: null,
                    pageSize: null
                })
            });

            if (!response.ok) {
                const text = await response.text();
                throw new Error(text || "Xuất file thất bại.");
            }

            const blob = await response.blob();
            const contentDisposition = response.headers.get("content-disposition") || "";
            const fileNameMatch = contentDisposition.match(/filename\*?=(?:UTF-8'')?"?([^";]+)"?/i);
            const fileName = fileNameMatch ? decodeURIComponent(fileNameMatch[1]) : `DataEmployeeEvaluation_${Date.now()}.xlsx`;

            const url = window.URL.createObjectURL(blob);
            const anchor = document.createElement("a");
            anchor.href = url;
            anchor.download = fileName;
            document.body.appendChild(anchor);
            anchor.click();
            anchor.remove();
            window.URL.revokeObjectURL(url);

            showNotify("success", "Thành công", "Xuất file Excel thành công.");
        } catch (err) {
            showNotify("error", "Lỗi", getErrorMessage(err));
        }
    }

    function showNotify(type, title, message) {
        let overlay = document.getElementById("customNotifyOverlay");
        if (!overlay) {
            overlay = document.createElement("div");
            overlay.id = "customNotifyOverlay";
            overlay.className = "custom-notify-overlay";
            overlay.innerHTML = `
                <div class="custom-notify-box">
                    <div id="customNotifyHead" class="custom-notify-head"></div>
                    <div id="customNotifyBody" class="custom-notify-body"></div>
                    <div class="custom-notify-foot">
                        <button type="button" id="customNotifyBtn" class="custom-notify-btn">Đóng</button>
                    </div>
                </div>`;
            document.body.appendChild(overlay);

            overlay.addEventListener("click", (e) => {
                if (e.target === overlay) {
                    overlay.style.display = "none";
                }
            });

            overlay.querySelector("#customNotifyBtn")?.addEventListener("click", () => {
                overlay.style.display = "none";
            });
        }

        const head = document.getElementById("customNotifyHead");
        const body = document.getElementById("customNotifyBody");
        head.className = `custom-notify-head ${type === "success" ? "success" : "error"}`;
        head.textContent = title;
        body.textContent = message;
        overlay.style.display = "flex";
    }

    function safeValue(value) {
        return value ?? "";
    }

    function formatScore(score) {
        if (score === null || score === undefined || Number.isNaN(Number(score))) {
            return "";
        }
        return Number(score).toFixed(2);
    }

    function formatDate(value) {
        if (!value) {
            return "";
        }
        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return "";
        }
        return new Intl.DateTimeFormat("vi-VN", {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit"
        }).format(date);
    }

    function escapeHtml(value) {
        if (value === null || value === undefined) {
            return "";
        }
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#39;");
    }

    function getErrorMessage(err) {
        if (err instanceof Error) {
            return err.message;
        }
        return "Đã xảy ra lỗi không xác định.";
    }
})();