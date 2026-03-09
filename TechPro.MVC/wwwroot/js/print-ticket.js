// Print Ticket Functionality
function printTicket(ticketId) {
    // Open ticket detail in new window for printing
    const url = `/KyThuat/ChiTiet/${ticketId}?print=true`;
    const printWindow = window.open(url, '_blank');
    
    printWindow.onload = function() {
        printWindow.print();
    };
}

function exportTicketPDF(ticketId) {
    if (typeof html2canvas === 'undefined' || typeof jsPDF === 'undefined') {
        toastr.error('Thư viện PDF chưa được tải. Vui lòng tải lại trang.');
        return;
    }

    toastr.info('Đang tạo PDF...');
    
    const element = document.querySelector('.ticket-detail-container') || document.body;
    
    html2canvas(element, {
        scale: 2,
        useCORS: true,
        logging: false
    }).then(canvas => {
        const imgData = canvas.toDataURL('image/png');
        const pdf = new jsPDF.jsPDF('p', 'mm', 'a4');
        const imgWidth = 210;
        const pageHeight = 295;
        const imgHeight = (canvas.height * imgWidth) / canvas.width;
        let heightLeft = imgHeight;
        let position = 0;

        pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
        heightLeft -= pageHeight;

        while (heightLeft >= 0) {
            position = heightLeft - imgHeight;
            pdf.addPage();
            pdf.addImage(imgData, 'PNG', 0, position, imgWidth, imgHeight);
            heightLeft -= pageHeight;
        }

        pdf.save(`Phieu_${ticketId}_${new Date().toISOString().split('T')[0]}.pdf`);
        toastr.success('Đã xuất PDF thành công!');
    }).catch(err => {
        console.error('PDF export error:', err);
        toastr.error('Có lỗi khi xuất PDF.');
    });
}

