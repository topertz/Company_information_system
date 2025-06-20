$(document).ready(function () {
    $('#showRegister').click(function () {
        $('#login').hide();
        $('#loginForm').hide();
        $(this).parent().hide();

        $('#register').show();
        $('#registerForm').show();
        $('#backToLoginP').show();

        $('#loginText').text('Beregisztrálás a rendszerbe');
        $('html, body').animate({ scrollTop: $('#register').offset().top }, 500);
    });

    $('#showLogin').click(function () {
        $('#register').hide();
        $('#registerForm').hide();
        $('#backToLoginP').hide();

        $('#login').show();
        $('#loginForm').show();
        $('#showRegister').parent().show();

        $('#loginText').text('Belépés a rendszerbe');
        $('html, body').animate({ scrollTop: $('#login').offset().top }, 500);
    });

    $('#loginText').click(function (e) {
        e.preventDefault();

        if ($('#loginText').text().trim() === 'Beregisztrálás a rendszerbe') {
            $('html, body').animate({ scrollTop: $('#register').offset().top }, 500);
        } else {
            $('html, body').animate({ scrollTop: $('#login').offset().top }, 500);
        }
    });

    var role = localStorage.getItem("userRole");

    if (role === "customer") {
        $('#finance').hide();
        $('#report').hide();
    } else if (role === "admin") {
        $('#finance').show();
        $('#report').show();
    }
});

$(document).ready(function () {

    $('#backButton').click(function (e) {
        e.preventDefault();

        $('#btn-products').prop('disabled', false);
        $('#btn-orders').prop('disabled', true);
        $('#btn-finance').prop('disabled', true);
    });

    $.get('/User/CheckSession')
        .done(function (response) {
            if (response.userID !== -1) {
                user = { id: response.userID };
            } else {
                user = null;
            }
        })
        .fail(function () {
            console.error('Session check failed.');
        });

    $('#registerForm').submit(function (e) {
        e.preventDefault();

        const username = $('#reg-username').val().trim();
        const password = $('#reg-password').val().trim();
        const role = $('#reg-role').val().trim();

        if (username && password && role) {
            $.post('/User/Create', { username, password, role }, function () {
                alert('Sikeres regisztráció! Most már bejelentkezhetsz.');
                $('#registerForm')[0].reset();
                $('#register').css('display', 'none');
                $('#registerForm').css('display', 'none');
                $('#login').css('display', 'block');
                $('#loginForm').css('display', 'block');
                $('#loginText').text('Belépés a rendszerbe');
                $('#loginText').attr('href', '#login');
                $('html, body').animate({
                    scrollTop: $('#loginForm').offset().top
                }, 800);
            }).fail(function (error) {
                alert('Hiba a regisztráció során!');
                console.error(error);
            });
        } else {
            alert('Kérlek, tölts ki minden mezőt!');
        }
    });

    const loginForm = $('#loginForm');
    if (loginForm.length) {
        loginForm.submit(function (e) {
            e.preventDefault();

            const username = $('#username').val().trim();
            const password = $('#password').val().trim();

            $.post('/User/Login', { username, password }, function (data) {
                alert('Sikeres bejelentkezés!');
                localStorage.setItem('userRole', data.role); 
                localStorage.setItem('userID', data.userID);
                window.location.href = 'dashboard.html';
            }).fail(function () {
                alert('Hibás felhasználónév vagy jelszó!');
            });
        });
    }

    $('#logoutButton').click(function () {
        $.post('/User/Logout')
            .done(function () {
                localStorage.removeItem('username');
            })
            .fail(function (xhr, status, error) {
                console.error('Hiba a kijelentkezés során:', status, error);
                alert('Hiba történt a kijelentkezés során. Kérjük, próbálja újra!');
            });
    });

    const btnOrders = $('#btn-orders');
    const btnFinance = $('#btn-finance');
    $('#btn-products').click(function () {
        window.location.href = 'products.html';
    });

    $('#btn-orders').click(function () {
        window.location.href = 'orders.html';
    });

    $('#btn-finance').click(function () {
        window.location.href = 'finance.html';
    });

    const form = $('#addProductForm');
    const nameInput = $('#productName');
    const priceInput = $('#productPrice');
    const imageUrlInput = $('#productImageUrl');

    const allowedProducts = {
        'Gerbera': '/images/gerbera.png',
        'Liliom': '/images/liliom.png',
        'Rózsa': '/images/roza.png',
        'Tulipán': '/images/tulipan.png',
        'Kaktusz': '/images/kaktusz.png',
        'Pipacs': '/images/pipacs.png'
    };

    const tableBody = $('#productTableBody');

    function fetchProducts() {
        $.get('/Product/GetAllProducts', function (products) {
            const select = $('#productID');
            select.empty().append('<option value="">Válassz virágot</option>');
            products.forEach(product => {
                const option = $('<option>')
                    .val(product.productID)
                    .text(product.productName)
                    .attr('data-price', product.productPrice);
                select.append(option);
            });
        });
    }
    $('#productID').on('change', function () {
        const price = $(this).find('option:selected').data('price');
        $('#price').val(price ?? '');
        $('#amount').val(1);
    });

    var userRole = localStorage.getItem("userRole");
    function renderProducts(products) {
        const gallery = $('#productGallery');
        gallery.empty();

        const replaceAccents = (str) => {
            const accentsMap = {
                'á': 'a', 'é': 'e', 'í': 'i', 'ó': 'o', 'ö': 'o', 'ő': 'o',
                'ú': 'u', 'ü': 'u', 'ű': 'u', 'Á': 'A', 'É': 'E', 'Í': 'I',
                'Ó': 'O', 'Ö': 'O', 'Ő': 'O', 'Ú': 'U', 'Ü': 'U', 'Ű': 'U'
            };
            return str.replace(/[áéíóöőúüűÁÉÍÓÖŐÚÜŰ]/g, match => accentsMap[match] || match);
        };

        products.forEach(product => {
            const imageUrl = `/images/${replaceAccents(product.productName.toLowerCase().replace(/\s+/g, '-'))}.png`;
            const card = `
            <div class="col">
                <div class="card h-100 shadow-sm">
                    <img src="${imageUrl}" class="card-img-top" alt="${product.productName}" style="height: 200px; object-fit: cover;">
                    <div class="card-body">
                        <h5 class="card-title">${product.productName}</h5>
                        <p class="card-text fw-bold">${product.productPrice} Ft</p>
                        ${userRole === 'admin' ? `
                            <button class="btn btn-warning edit-product" data-id="${product.productID}" data-name="${product.productName}" data-price="${product.productPrice}">Módosítás</button>
                            <button class="btn btn-danger delete-product" data-id="${product.productID}">Törlés</button>
                        ` : ''}
                    </div>
                </div>
            </div>
        `;
            gallery.append(card);
        });
    }
    var userRole = localStorage.getItem("userRole");

    if (userRole === 'admin') {
        $('#addProductButtonContainer').show();
    }

    $('#showAddProductForm').click(function () {
        $('#addProductFormContainer').toggle();
    });

    nameInput.change(function () {
        const selectedProduct = nameInput.val();
        const imageUrl = allowedProducts[selectedProduct] || '';

        imageUrlInput.val(imageUrl);
    });

    form.submit(function (e) {
        e.preventDefault();

        const productName = nameInput.val().trim();
        const productPrice = parseInt(priceInput.val());
        const imageUrl = imageUrlInput.val().trim();

        if (productName && !isNaN(productPrice) && imageUrl) {
            $.post('/Product/CreateProduct', { productName: productName, productPrice: productPrice, imageUrl: imageUrl }, function () {
                alert('Termék sikeresen hozzáadva!');
                form[0].reset();
                location.reload();
                fetchProducts();
            }).fail(function (error) {
                console.error('Hiba a termék hozzáadásakor:', error);
            });
        } else {
            alert('Kérlek, válassz egy terméket és add meg az árat!');
        }
    });

    $(document).on('click', '.edit-product', function () {
        const id = $(this).data('id');
        const price = $(this).data('price');

        $('#editProductPrice').val(price);

        $('#editProductForm').data('productId', id);
        $('#editProductModal').modal('show');
    });

    $('#editProductForm').submit(function (e) {
        e.preventDefault();

        const productId = $(this).data('productId');
        const productPrice = parseInt($('#editProductPrice').val());

        if (productName && !isNaN(productPrice)) {
            $.post('/Product/UpdateProduct', { productID: productId, productPrice: productPrice }, function (response) {
                alert('Product updated successfully:', response);
                $('#editProductModal').modal('hide');
                location.reload();
                fetchProducts();
            }).fail(function (error) {
                console.error('Hiba a termék módosításakor:', error);
            });
        }
    });

    $(document).on('click', '.delete-product', function () {
        const id = $(this).data('id');
        if (confirm('Biztosan törlöd ezt a terméket?')) {
            $.post('/Product/DeleteProduct', { productID: id }, function (response) {
                alert('Product deleted successfully:', response);
                location.reload();
                fetchProducts();
            }).fail(function (error) {
                console.error('Hiba a törlés során:', error);
            });
        }
    });

    function seedIfEmpty() {
        $.get('/Product/GetAllProducts')
            .done(function (products) {
                if (!products || products.length === 0) {
                    $.post('/Product/SeedProducts')
                        .done(function () {
                            fetchProducts();
                        })
                        .fail(function (error) {
                            console.error('Hiba a seedeléskor:', error);
                        });
                } else {
                    renderProducts(products);
                }
            })
            .fail(function (error) {
                console.error('Hiba a termékek lekérésekor:', error);
            });
    }
    seedIfEmpty();
    fetchProducts();

    const ordersTable = $('#ordersTableBody');
    const orderForm = $('#addOrderForm');
    const customerInput = $('#customerName');
    const customerEmailInput = $('#customerEmail');
    const customerAddressInput = $('#customerAddress');
    const houseNumberInput = $('#houseNumber');
    const productIDInput = $('#productID');
    const priceProduct = $('#price');
    const productAmount = $('#amount');
    const phoneNumberInput = $('#phoneNumber'); 
    function fetchOrders() {
        var userID = localStorage.getItem("userID");
        var role = localStorage.getItem("userRole");
        $.get('/Order/GetAllOrders', { userRole: role, userID: userID }, function (data) {
            renderOrders(data);
        }).fail(function (error) {
            console.error('Hiba a rendelések lekérésekor:', error);
        });
    }

    orderForm.submit(function (e) {
        e.preventDefault();
        var userID = localStorage.getItem("userID");
        const customerName = customerInput.val().trim();
        const customerEmail = customerEmailInput.val().trim();
        const customerAddress = customerAddressInput.val().trim();
        const houseNumber = houseNumberInput.val().trim();
        const phoneNumber = phoneNumberInput.val().trim();
        const productID = parseInt(productIDInput.val());
        const price = parseInt(priceProduct.val());
        const amount = parseInt(productAmount.val());
        const orderDate = new Date().toISOString().split('T')[0];
        const editingId = orderForm.data('editing-id');

        if (customerName && !isNaN(productID) && !isNaN(price)) {
            const orderData = {
                UserID: userID,
                CustomerName: customerName,
                CustomerEmail: customerEmail,
                CustomerAddress: customerAddress,
                HouseNumber: houseNumber,
                PhoneNumber: phoneNumber,
                ProductID: productID,
                Price: price,
                Amount: amount,
                Status: "Rendelés létrehozva",
                OrderDate: orderDate,
            };

            if (editingId) {
                orderData.OrderID = editingId;
                orderData.Status = "Rendelés módosítva"; 
                $.post('/Order/UpdateOrder', orderData, function () {
                    alert('Rendelés módosítva!');
                    orderForm[0].reset();
                    orderForm.removeData('editing-id');
                    $('button[type="submit"]').text('Rendelés hozzáadása'); 
                    fetchOrders();
                }).fail(function (error) {
                    console.error('Hiba a rendelés frissítésekor:', error);
                });
            } else {
                $.post('/Order/CreateOrder', orderData, function () {
                    alert('Rendelés létrehozva!');
                    orderForm[0].reset();
                    $('button[type="submit"]').text('Rendelés hozzáadása');
                    fetchOrders();
                }).fail(function (error) {
                    console.error('Hiba a rendelés hozzáadásakor:', error);
                });
            }
        }
    });

    $(document).on('click', '.edit-order', function () {
        const id = $(this).data('id');

        $.get(`/Order/GetOrderById`, { orderID: id }, function (order) {
            customerInput.val(order.customerName);
            customerEmailInput.val(order.customerEmail);
            customerAddressInput.val(order.customerAddress);
            houseNumberInput.val(order.houseNumber);
            phoneNumberInput.val(order.phoneNumber);
            productIDInput.val(order.productID);
            priceProduct.val(order.price);
            productAmount.val(order.amount);

            orderForm.data('editing-id', id);
            $('button[type="submit"]').text('Rendelés módosítása');
        }).fail(function (error) {
            console.error('Hiba a rendelés betöltésekor:', error);
        });
    });

    $(document).on('click', '.delete-order', function () {
        const id = $(this).data('id');
        if (confirm('Biztosan törlöd ezt a rendelést?')) {
            $.post('/Order/DeleteOrder', { orderID: id }, function () {
                fetchOrders();
            }).fail(function (error) {
                console.error('Hiba a rendelés törlésekor:', error);
            });
        }
    });

    function renderOrders(orders) {
        ordersTable.empty();
        orders.forEach(order => {
            const row = `
            <tr>
                <td>${order.customerName}</td>
                <td>${order.customerEmail}</td>
                <td>${order.customerAddress}</td>
                <td>${order.houseNumber}</td> 
                <td>${order.phoneNumber}</td> 
                <td>${order.productName}</td>
                <td>${order.price} Ft</td>
                <td>${order.amount}</td>
                <td>${order.status}</td>
                <td>${order.orderDate.split('T')[0]}</td>
                <td>
                    <button class="btn btn-warning btn-sm edit-order" data-id="${order.orderID}" style="width: 100%; margin-bottom: 5px;">
                        ✏️ Szerkesztés
                    </button>
                    <button class="btn btn-danger btn-sm delete-order" data-id="${order.orderID}" style="width: 100%;">
                        🗑 Törlés
                    </button>
                </td>
            </tr>
        `;
            ordersTable.append(row);
        });
    }

    fetchOrders();

    const financeTable = $('#financeTableBody');
    function fetchFinance() {
        $.get('/Finance/GetAllFinance', function (data) {
            renderFinance(data);
            populateProductDropdown();
        }).fail(function (error) {
            console.error('Hiba a pénzügyi adatok lekérésekor:', error);
        });
    }

    function populateProductDropdown() {
        $.get('/Product/GetAllProducts', function (products) {
            const productSelect = $('#productID');
            productSelect.empty().append('<option value="">Válassz terméket</option>');
            products.forEach(product => {
                productSelect.append(`<option value="${product.productID}" data-price="${product.productPrice}">${product.productName}</option>`);
            });
        }).fail(function (error) {
            console.error('Hiba a terméklista betöltésekor:', error);
        });
    }

    function renderFinance(entries) {
        financeTable.empty();

        let totalIncome = 0;
        let totalExpense = 0;

        entries.forEach(entry => {
            totalIncome += entry.income;
            totalExpense += entry.expense;

            const row = `
                <tr>
                    <td>${entry.month}</td>
                    <td>${entry.income} Ft</td>
                    <td>${entry.expense} Ft</td>
                    <td>
                    <button class="btn btn-danger btn-sm delete-finance" data-id="${entry.financeID}">
                        🗑 Törlés
                    </button>
                </td>
                </tr>
            `;
            financeTable.append(row);
        });

        $('#income-amount').text(totalIncome + ' Ft');
        $('#expense-amount').text(totalExpense + ' Ft');
        $('#profit-amount').text((totalIncome - totalExpense) + ' Ft');
    }

    fetchFinance();

    $('#addFinanceForm').submit(function (e) {
        e.preventDefault();

        const month = $('#month').val().trim();
        const income = parseInt($('#income').val());
        const expense = parseInt($('#expense').val());

        if (month && !isNaN(income) && !isNaN(expense)) {
            $.post('/Finance/CreateFinance', { month: month, income: income, expense: expense }, function () {
                $('#addFinanceForm')[0].reset();
                fetchFinance();
            }).fail(function (error) {
                console.error('Hiba a pénzügyi adat hozzáadásakor:', error);
            });
        } else {
            alert('Kérlek, töltsd ki az összes mezőt!');
        }
    });

    $(document).on('click', '.delete-finance', function () {
        const id = $(this).data('id');
        if (confirm('Biztosan törlöd ezt a pénzügyi adatot?')) {
            $.post('/Finance/DeleteFinance', { financeID: id }, function () {
                fetchFinance();
            }).fail(function (error) {
                console.error('Hiba a pénzügyi adat törlésekor:', error);
            });
        }
    });

    $.post('/Finance/GenerateMonthlyFinanceFromOrders', function () {
        fetchFinance();
    }).fail(function () {
        console.error('Hiba a pénzügyi adatok automatikus frissítésekor.');
    });
});

$(document).ready(function () {
    $('#exportPDF').click(function () {
        const ordersTableBody = document.querySelector('#ordersTableBody');

        if (ordersTableBody && ordersTableBody.rows.length === 0) {
            alert("Nincs rendelés, amit PDF-be menthetnél.");
            return;
        }

        const originalTable = document.querySelector('table');
        const clone = originalTable.cloneNode(true);

        clone.querySelectorAll('button').forEach(btn => btn.remove());

        const thead = clone.querySelector('thead tr');
        const tbodyRows = clone.querySelectorAll('tbody tr');

        var userRole = localStorage.getItem("userRole");

        if (!thead.querySelector('th:last-child').textContent.includes('Végösszeg')) {
            const newTh = document.createElement('th');
            newTh.textContent = 'Végösszeg';
            thead.appendChild(newTh);
        }

        tbodyRows.forEach(row => {
            const cells = row.querySelectorAll('td');
            let price = 0, qty = 0;

            if (cells.length >= 8) {
                const priceText = cells[6].textContent.trim();
                const qtyText = cells[7].textContent.trim();

                const priceMatch = priceText.match(/(\d+(?:[.,]\d+)?)/);
                price = priceMatch ? parseFloat(priceMatch[1].replace(',', '.')) : 0;
                qty = parseFloat(qtyText) || 0;

                const total = (price * qty).toFixed(2) + ' Ft';

                if (cells.length === 9) {
                    const newTd = document.createElement('td');
                    newTd.textContent = total;
                    row.appendChild(newTd);
                } else {
                    cells[10].textContent = total;
                }

                const statusCell = cells[8];
                statusCell.textContent = "Szállításra kész";
            } else {
                const newTd = document.createElement('td');
                newTd.textContent = '—';
                row.appendChild(newTd);
            }
        });

        const thElements = clone.querySelectorAll('th');
        thElements.forEach((th, index) => {
            const columnWidths = [
                '120px', '150px', '120px', '80px', '120px',
                '80px', '120px', '100px', '80px', '120px'
            ];
            th.style.width = columnWidths[index] || 'auto';
        });

        clone.style.tableLayout = 'auto';
        clone.style.width = '100%';

        clone.style.border = '1px solid #444';
        clone.style.borderCollapse = 'collapse';

        clone.querySelectorAll('th, td').forEach(cell => {
            cell.style.border = '1px solid #444';
            cell.style.padding = '3px';
            cell.style.margin = '0';
            cell.style.textAlign = 'center';
            cell.style.fontSize = '10px';
        });

        const wrapper = document.createElement('div');
        wrapper.style.left = '0';
        wrapper.style.position = 'fixed';
        wrapper.style.top = '-10000px';
        wrapper.style.width = '800px';
        wrapper.appendChild(clone);
        document.body.appendChild(wrapper);

        html2canvas(wrapper, {
            backgroundColor: "#ffffff",
            scale: 2
        }).then(canvas => {
            document.body.removeChild(wrapper);
            const imgData = canvas.toDataURL('image/png');
            const { jsPDF } = window.jspdf;
            const pdf = new jsPDF('p', 'mm', 'a4');
            const pdfWidth = pdf.internal.pageSize.getWidth();
            const imgProps = pdf.getImageProperties(imgData);
            const imgWidth = pdfWidth * 0.9;
            const imgHeight = (imgProps.height * imgWidth) / imgProps.width;
            const x = (pdfWidth - imgWidth) / 2;

            if (userRole === 'admin') {
                pdf.addImage(imgData, 'PNG', x, 20, imgWidth, imgHeight);
                pdf.save('rendelesek_admin.pdf');
            } else {
                pdf.addImage(imgData, 'PNG', x, 20, imgWidth, imgHeight);
                pdf.save('rendelesek_vasarlo.pdf');
            }
        });
    });
});