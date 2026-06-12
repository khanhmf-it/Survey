document.addEventListener("DOMContentLoaded", () => {
    const form = document.querySelector(".survey-page form");
    const localizedNode = document.getElementById("index-localized-text");
    const t = {
        submitSuccessTitle: localizedNode?.dataset.submitSuccessTitle || "Success",
        submitSuccessMessage: localizedNode?.dataset.submitSuccessMessage || "Submit successful.",
        submitFailedTitle: localizedNode?.dataset.submitFailedTitle || "Failed",
        submitFailedMessage: localizedNode?.dataset.submitFailedMessage || "Submit failed.",
        submitFailedFallback: localizedNode?.dataset.submitFailedFallback || "An error occurred while submitting.",
        modalCloseButton: localizedNode?.dataset.modalCloseButton || "Close",
        mergeGoodSituationLabel: localizedNode?.dataset.mergeGoodSituationLabel || "Good point",
        mergeImproveSituationLabel: localizedNode?.dataset.mergeImproveSituationLabel || "Need improve",
        mergeGoodProposalLabel: localizedNode?.dataset.mergeGoodProposalLabel || "Keep doing",
        mergeImproveProposalLabel: localizedNode?.dataset.mergeImproveProposalLabel || "Improvement proposal"
        ,submitMissingScoresMessage: localizedNode?.dataset.submitMissingScoresMessage || "Vui lòng điền đủ điểm cho 5 nhóm tiêu chí."
    };

    if (!form) {
        return;
    }

    let isSubmitting = false;

    form.addEventListener("keydown", (event) => {
        if (event.key !== "Enter" || event.isComposing) {
            return;
        }

        const target = event.target;

        if (target instanceof HTMLTextAreaElement || target instanceof HTMLButtonElement) {
            return;
        }

        event.preventDefault();
    });

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

            // Validate that all 5 groups have both good and improve scores filled
            const requiredGroups = 5;
            let missingScores = false;
            for (let i = 0; i < requiredGroups; i++) {
                const goodKey = `g${i + 1}_good_score`;
                const improveKey = `g${i + 1}_improve_score`;
                if (payload[goodKey] === null || payload[improveKey] === null) {
                    missingScores = true;
                    break;
                }
            }

            if (missingScores) {
                showResultModal({
                    success: false,
                    title: t.submitFailedTitle,
                    message: t.submitMissingScoresMessage
                });

                isSubmitting = false;
                if (submitButton) {
                    submitButton.disabled = false;
                }

                return;
            }
            const response = await fetch("/Home/SendMailSectionManager", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(payload)
            });

            if (!response.ok) {
                const errorText = await response.text();
                throw new Error(errorText || t.submitFailedMessage);
            }

            showResultModal({
                success: true,
                title: t.submitSuccessTitle,
                message: t.submitSuccessMessage
            });

            form.reset();
        } catch (error) {
            showResultModal({
                success: false,
                title: t.submitFailedTitle,
                message: error?.message || t.submitFailedFallback
            });
        } finally {
            isSubmitting = false;
            if (submitButton) {
                submitButton.disabled = false;
            }
        }
    });

    function buildPayload() {
        const employeeName = (document.getElementById("employee_name")?.value || "").trim();
        const department = (document.getElementById("employee_group")?.value || "").trim();

        const groups = Array.from(document.querySelectorAll(".evaluation-groups .group-card"));

        const groupValues = groups.map((group) => {
            const textInputs = group.querySelectorAll(".text-input");
            const scoreSelects = group.querySelectorAll(".score-select");

            const groupScore = parseNullableInt(scoreSelects[0]?.value);
            const goodScore = groupScore;
            const improveScore = parseNullableInt(scoreSelects[1]?.value) ?? groupScore;

            return {
                goodPoint: (textInputs[0]?.value || "").trim(),
                goodScore,
                improvePoint: (textInputs[3]?.value || "").trim(),
                improveScore,
                goodSituation: (textInputs[1]?.value || "").trim(),
                improveSituation: (textInputs[4]?.value || "").trim(),
                goodProposal: (textInputs[2]?.value || "").trim(),
                improveProposal: (textInputs[5]?.value || "").trim()
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
            employee_code: null,
            employee_name: employeeName || null,
            department: department || null,
            g1_good_point: groupValues[0]?.goodPoint || null,
            g1_good_score: groupValues[0]?.goodScore ?? null,
            g1_improve_point: groupValues[0]?.improvePoint || null,
            g1_improve_score: groupValues[0]?.improveScore ?? null,
            g1_example_good: groupValues[0]?.goodSituation ?? null,
            g1_example_improve:  groupValues[0]?.improveSituation ?? null,
            g1_improvement_proposal_good:  groupValues[0]?.goodProposal,
            g1_improvement_proposal_improve: groupValues[0]?.improveProposal,
            g2_good_point: groupValues[1]?.goodPoint || null,
            g2_good_score: groupValues[1]?.goodScore ?? null,
            g2_improve_point: groupValues[1]?.improvePoint || null,
            g2_improve_score: groupValues[1]?.improveScore ?? null,
            g2_example_good: groupValues[1]?.goodSituation ?? null,
            g2_example_improve: groupValues[1]?.improveSituation ?? null,
            g2_improvement_proposal_good:  groupValues[1]?.goodProposal,
            g2_improvement_proposal_improve: groupValues[1]?.improveProposal,
            g3_good_point: groupValues[2]?.goodPoint || null,
            g3_good_score: groupValues[2]?.goodScore ?? null,
            g3_improve_point: groupValues[2]?.improvePoint || null,
            g3_improve_score: groupValues[2]?.improveScore ?? null,
            g3_example_good: groupValues[2]?.goodSituation ?? null,
            g3_example_improve: groupValues[2]?.improveSituation ?? null,
            g3_improvement_proposal_good:  groupValues[2]?.goodProposal,
            g3_improvement_proposal_improve: groupValues[2]?.improveProposal,
            g4_good_point: groupValues[3]?.goodPoint || null,
            g4_good_score: groupValues[3]?.goodScore ?? null,
            g4_improve_point: groupValues[3]?.improvePoint || null,
            g4_improve_score: groupValues[3]?.improveScore ?? null,
            g4_example_good: groupValues[3]?.goodSituation ?? null,
            g4_example_improve: groupValues[3]?.improveSituation ?? null,
            g4_improvement_proposal_good:  groupValues[3]?.goodProposal,
            g4_improvement_proposal_improve: groupValues[3]?.improveProposal,
            g5_good_point: groupValues[4]?.goodPoint || null,
            g5_good_score: groupValues[4]?.goodScore ?? null,
            g5_improve_point: groupValues[4]?.improvePoint || null,
            g5_improve_score: groupValues[4]?.improveScore ?? null,
            g5_example_good: groupValues[4]?.goodSituation ?? null,
            g5_example_improve: groupValues[4]?.improveSituation ?? null,
            g5_improvement_proposal_good:  groupValues[4]?.goodProposal,
            g5_improvement_proposal_improve: groupValues[4]?.improveProposal,
            improvement_proposal: proposal || null,
            total_score: totalScore,
            created_at: new Date().toISOString()
        };
    }

    function mergeSituations(goodSituation, improveSituation) {
        const sections = [];
        if (goodSituation) sections.push(`${t.mergeGoodSituationLabel}: ${goodSituation}`);
        if (improveSituation) sections.push(`${t.mergeImproveSituationLabel}: ${improveSituation}`);
        return sections.length > 0 ? sections.join("\n") : null;
    }

    function mergeProposals(goodProposal, improveProposal) {
        const sections = [];
        if (goodProposal) sections.push(`${t.mergeGoodProposalLabel}: ${goodProposal}`);
        if (improveProposal) sections.push(`${t.mergeImproveProposalLabel}: ${improveProposal}`);
        return sections.length > 0 ? sections.join("\n") : null;
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
            button.textContent = t.modalCloseButton;
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