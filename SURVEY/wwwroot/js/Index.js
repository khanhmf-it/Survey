document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector(".survey-page form");

    if (!form) {
        return;
    }

    let isSubmitting = false;

    form.addEventListener("submit", async (event) => {
        event.preventDefault();

        if (isSubmitting) {
            return;
        }

        const submitButton = form.querySelector(".submit-btn");

        try {
            isSubmitting = true;
            if (submitButton) {
                submitButton.disabled = true;
            }

            const payload = buildPayload();
            const response = await fetch("/Home/AddEvaluation", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || "Submit failed.");
            }

            showResultModal({
                success: true,
                title: "Thành công",
                message: "Gửi đánh giá thành công."
            });

            form.reset();
        } catch (error) {
            showResultModal({
                success: false,
                title: "Thất bại",
                message: error?.message || "Có lỗi xảy ra khi gửi đánh giá."
            });
        } finally {
            isSubmitting = false;
            if (submitButton) {
                submitButton.disabled = false;
            }
        }
    });

    function buildPayload() {
        const employeeCode = (document.getElementById("employee_id")?.value || "").trim();
        const department = (document.getElementById("employee_group")?.value || "").trim();

        const groups = Array.from(document.querySelectorAll(".evaluation-groups .group-card"));

        const groupValues = groups.map((group) => {
            const textInputs = group.querySelectorAll(".text-input");
            const scoreSelects = group.querySelectorAll(".score-select");

            return {
                goodPoint: (textInputs[0]?.value || "").trim(),
                goodScore: parseNullableInt(scoreSelects[0]?.value),
                improvePoint: (textInputs[1]?.value || "").trim(),
                improveScore: parseNullableInt(scoreSelects[1]?.value),
                example: (textInputs[2]?.value || "").trim()
            };
        });

        const proposal = (document.querySelector(".proposal-textarea")?.value || "").trim();

        const allScores = groupValues
            .flatMap((g) => [g.goodScore, g.improveScore])
            .filter((score) => score !== null);

        const totalScore = allScores.length > 0
            ? Number((allScores.reduce((sum, score) => sum + score, 0) / allScores.length).toFixed(2))
            : null;

        return {
            employee_code: employeeCode || null,
            department: department || null,
            g1_good_point: groupValues[0]?.goodPoint || null,
            g1_good_score: groupValues[0]?.goodScore ?? null,
            g1_improve_point: groupValues[0]?.improvePoint || null,
            g1_improve_score: groupValues[0]?.improveScore ?? null,
            g1_example: groupValues[0]?.example || null,
            g2_good_point: groupValues[1]?.goodPoint || null,
            g2_good_score: groupValues[1]?.goodScore ?? null,
            g2_improve_point: groupValues[1]?.improvePoint || null,
            g2_improve_score: groupValues[1]?.improveScore ?? null,
            g2_example: groupValues[1]?.example || null,
            g3_good_point: groupValues[2]?.goodPoint || null,
            g3_good_score: groupValues[2]?.goodScore ?? null,
            g3_improve_point: groupValues[2]?.improvePoint || null,
            g3_improve_score: groupValues[2]?.improveScore ?? null,
            g3_example: groupValues[2]?.example || null,
            g4_good_point: groupValues[3]?.goodPoint || null,
            g4_good_score: groupValues[3]?.goodScore ?? null,
            g4_improve_point: groupValues[3]?.improvePoint || null,
            g4_improve_score: groupValues[3]?.improveScore ?? null,
            g4_example: groupValues[3]?.example || null,
            g5_good_point: groupValues[4]?.goodPoint || null,
            g5_good_score: groupValues[4]?.goodScore ?? null,
            g5_improve_point: groupValues[4]?.improvePoint || null,
            g5_improve_score: groupValues[4]?.improveScore ?? null,
            g5_example: groupValues[4]?.example || null,
            improvement_proposal: proposal || null,
            total_score: totalScore,
            created_at: new Date().toISOString()
        };
    }

    function parseNullableInt(value) {
        const parsed = Number.parseInt(value, 10);
        return Number.isNaN(parsed) ? null : parsed;
    }

    function showResultModal({ success, title, message }) {
        let overlay = document.getElementById("custom-result-modal");

        if (!overlay) {
            overlay = document.createElement("div");
            overlay.id = "custom-result-modal";
            overlay.style.position = "fixed";
            overlay.style.inset = "0";
            overlay.style.background = "rgba(0, 0, 0, 0.45)";
            overlay.style.display = "flex";
            overlay.style.alignItems = "center";
            overlay.style.justifyContent = "center";
            overlay.style.zIndex = "9999";

            const modal = document.createElement("div");
            modal.style.width = "min(90%, 420px)";
            modal.style.background = "#fff";
            modal.style.borderRadius = "14px";
            modal.style.padding = "20px";
            modal.style.boxShadow = "0 10px 30px rgba(0,0,0,0.2)";
            modal.style.textAlign = "center";

            const icon = document.createElement("div");
            icon.id = "custom-result-modal-icon";
            icon.style.fontSize = "32px";
            icon.style.marginBottom = "10px";

            const titleEl = document.createElement("h3");
            titleEl.id = "custom-result-modal-title";
            titleEl.style.margin = "0 0 8px";

            const messageEl = document.createElement("p");
            messageEl.id = "custom-result-modal-message";
            messageEl.style.margin = "0 0 16px";

            const button = document.createElement("button");
            button.type = "button";
            button.textContent = "Đóng";
            button.style.border = "none";
            button.style.padding = "10px 18px";
            button.style.borderRadius = "8px";
            button.style.cursor = "pointer";
            button.style.color = "#fff";
            button.addEventListener("click", () => {
                overlay.style.display = "none";
            });

            modal.appendChild(icon);
            modal.appendChild(titleEl);
            modal.appendChild(messageEl);
            modal.appendChild(button);
            overlay.appendChild(modal);

            overlay.addEventListener("click", (e) => {
                if (e.target === overlay) {
                    overlay.style.display = "none";
                }
            });

            document.body.appendChild(overlay);
        }

        const iconEl = overlay.querySelector("#custom-result-modal-icon");
        const titleEl = overlay.querySelector("#custom-result-modal-title");
        const messageEl = overlay.querySelector("#custom-result-modal-message");
        const closeBtn = overlay.querySelector("button");

        if (iconEl) {
            iconEl.textContent = success ? "✔" : "✖";
            iconEl.style.color = success ? "#16a34a" : "#dc2626";
        }

        if (titleEl) {
            titleEl.textContent = title;
        }

        if (messageEl) {
            messageEl.textContent = message;
        }

        if (closeBtn) {
            closeBtn.style.background = success ? "#16a34a" : "#dc2626";
        }

        overlay.style.display = "flex";
    }
});