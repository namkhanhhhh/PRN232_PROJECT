// Employer Dashboard JavaScript Functions

// Initialize when document is ready
$(document).ready(() => {
    initializeEmployerDashboard()
})

function initializeEmployerDashboard() {
    // Initialize filter functionality
    initializeFilters()

    // Initialize job post actions
    initializeJobPostActions()

    // Initialize form validations
    initializeFormValidations()

    // Initialize tooltips and popovers
    initializeTooltips()
}

// Filter functionality
function initializeFilters() {
    // Status filter tabs
    $(".filter-tab").on("click", function (e) {
        e.preventDefault()

        // Update active tab
        $(".filter-tab").removeClass("active")
        $(this).addClass("active")

        // Get filter values
        const status = $(this).data("status")
        const search = $("#searchInput").val()
        const fromDate = $("#fromDate").val()
        const toDate = $("#toDate").val()

        // Apply filters
        applyFilters(status, search, fromDate, toDate)
    })

    // Search functionality
    $("#searchInput").on(
        "input",
        debounce(function () {
            const status = $(".filter-tab.active").data("status") || "all"
            const search = $(this).val()
            const fromDate = $("#fromDate").val()
            const toDate = $("#toDate").val()

            applyFilters(status, search, fromDate, toDate)
        }, 500),
    )

    // Date range filters
    $("#fromDate, #toDate").on("change", () => {
        const status = $(".filter-tab.active").data("status") || "all"
        const search = $("#searchInput").val()
        const fromDate = $("#fromDate").val()
        const toDate = $("#toDate").val()

        applyFilters(status, search, fromDate, toDate)
    })

    // Filter button
    $("#filterBtn").on("click", () => {
        const status = $(".filter-tab.active").data("status") || "all"
        const search = $("#searchInput").val()
        const fromDate = $("#fromDate").val()
        const toDate = $("#toDate").val()

        applyFilters(status, search, fromDate, toDate)
    })
}

// Apply filters function
function applyFilters(status, search, fromDate, toDate) {
    const params = new URLSearchParams()

    if (status && status !== "all") {
        params.append("status", status)
    }

    if (search) {
        params.append("search", search)
    }

    if (fromDate) {
        params.append("fromDate", fromDate)
    }

    if (toDate) {
        params.append("toDate", toDate)
    }

    // Redirect with filters
    window.location.href = "/JobPostManagement?" + params.toString()
}

// Job post actions
function initializeJobPostActions() {
    // Status toggle confirmation
    $(".btn-toggle-status").on("click", function (e) {
        e.preventDefault()

        const form = $(this).closest("form")
        const currentStatus = $(this).data("current-status")
        const newStatus = currentStatus === "active" ? "inactive" : "active"
        const actionText = newStatus === "active" ? "kích hoạt" : "ẩn"

        if (confirm(`Bạn có chắc muốn ${actionText} bài đăng này?`)) {
            form.submit()
        }
    })

    // Delete confirmation
    $(".btn-delete").on("click", function (e) {
        e.preventDefault()

        const form = $(this).closest("form")

        if (confirm("Bạn có chắc muốn xóa bài đăng này? Hành động này không thể hoàn tác.")) {
            form.submit()
        }
    })

    // Card hover effects
    $(".job-card").hover(
        function () {
            $(this).addClass("shadow-lg")
        },
        function () {
            $(this).removeClass("shadow-lg")
        },
    )
}

// Form validations
function initializeFormValidations() {
    // Job post create/edit form validation
    $("#jobPostForm").on("submit", (e) => {
        let isValid = true

        // Clear previous errors
        $(".text-danger").text("")
        $(".form-control").removeClass("is-invalid")

        // Title validation
        const title = $("#Title").val().trim()
        if (!title) {
            showFieldError("Title", "Tiêu đề là bắt buộc")
            isValid = false
        }

        // Description validation
        const description = $("#Description").val().trim()
        if (!description) {
            showFieldError("Description", "Mô tả công việc là bắt buộc")
            isValid = false
        }

        // Location validation
        const location = $("#Location").val().trim()
        if (!location) {
            showFieldError("Location", "Địa điểm là bắt buộc")
            isValid = false
        }

        // Post type validation
        const postType = $('input[name="PostType"]:checked').val()
        if (!postType) {
            showFieldError("PostType", "Vui lòng chọn loại tin")
            isValid = false
        }

        // Salary validation
        const salaryMin = Number.parseFloat($("#SalaryMin").val()) || 0
        const salaryMax = Number.parseFloat($("#SalaryMax").val()) || 0

        if (salaryMin < 0) {
            showFieldError("SalaryMin", "Lương tối thiểu không được âm")
            isValid = false
        }

        if (salaryMax < 0) {
            showFieldError("SalaryMax", "Lương tối đa không được âm")
            isValid = false
        }

        if (salaryMax > 0 && salaryMin > salaryMax) {
            showFieldError("SalaryMax", "Lương tối đa phải lớn hơn lương tối thiểu")
            isValid = false
        }

        if (!isValid) {
            e.preventDefault()
            // Scroll to first error
            const firstError = $(".is-invalid").first()
            if (firstError.length) {
                $("html, body").animate(
                    {
                        scrollTop: firstError.offset().top - 100,
                    },
                    500,
                )
            }
        }
    })

    // Real-time validation
    $(".form-control").on("blur", function () {
        validateField($(this))
    })
}

// Show field error
function showFieldError(fieldName, message) {
    const field = $(`#${fieldName}`)
    field.addClass("is-invalid")
    field.siblings(".text-danger").text(message)
}

// Validate individual field
function validateField($field) {
    const fieldName = $field.attr("name") || $field.attr("id")
    const value = $field.val().trim()

    // Clear previous error
    $field.removeClass("is-invalid")
    $field.siblings(".text-danger").text("")

    // Required field validation
    if ($field.prop("required") && !value) {
        showFieldError(fieldName, "Trường này là bắt buộc")
        return false
    }

    return true
}

// Initialize tooltips
function initializeTooltips() {
    // Bootstrap tooltips
    $('[data-bs-toggle="tooltip"]').tooltip()

    // Custom tooltips for status badges
    $(".status-badge").each(function () {
        const status = $(this).data("status")
        let tooltipText = ""

        switch (status) {
            case "active":
                tooltipText = "Bài đăng đang hoạt động và hiển thị công khai"
                break
            case "inactive":
                tooltipText = "Bài đăng đã bị ẩn và không hiển thị công khai"
                break
            case "expired":
                tooltipText = "Bài đăng đã hết hạn nộp hồ sơ"
                break
        }

        $(this).attr("title", tooltipText).tooltip()
    })
}

// Utility functions
function debounce(func, wait) {
    let timeout
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout)
            func(...args)
        }
        clearTimeout(timeout)
        timeout = setTimeout(later, wait)
    }
}

// Show loading state
function showLoading(element) {
    const $element = $(element)
    $element.prop("disabled", true)

    const originalText = $element.text()
    $element.data("original-text", originalText)
    $element.html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...')
}

// Hide loading state
function hideLoading(element) {
    const $element = $(element)
    $element.prop("disabled", false)

    const originalText = $element.data("original-text")
    if (originalText) {
        $element.text(originalText)
    }
}

// Show success message
function showSuccessMessage(message) {
    const alertHtml = `
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            <i class="fas fa-check-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `

    $(".main-content").prepend(alertHtml)

    // Auto dismiss after 5 seconds
    setTimeout(() => {
        $(".alert-success").fadeOut()
    }, 5000)
}

// Show error message
function showErrorMessage(message) {
    const alertHtml = `
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            <i class="fas fa-exclamation-circle me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `

    $(".main-content").prepend(alertHtml)

    // Auto dismiss after 8 seconds
    setTimeout(() => {
        $(".alert-danger").fadeOut()
    }, 8000)
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat("vi-VN", {
        style: "currency",
        currency: "VND",
    }).format(amount)
}

// Format date
function formatDate(dateString) {
    const date = new Date(dateString)
    return date.toLocaleDateString("vi-VN", {
        year: "numeric",
        month: "2-digit",
        day: "2-digit",
    })
}

// Export functions for global use
window.EmployerDashboard = {
    showLoading,
    hideLoading,
    showSuccessMessage,
    showErrorMessage,
    formatCurrency,
    formatDate,
    applyFilters,
}
