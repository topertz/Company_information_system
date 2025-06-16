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

        if (username && password) {
            $.post('/User/Create', { username, password }, function () {
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
                localStorage.setItem('token', data.token);
                window.location.href = 'dashboard.html';
            }).fail(function () {
                alert('Hibás felhasználónév vagy jelszó!');
            });
        });
    }

    $('#logoutButton').click(function () {
        $.post('/User/Logout')
            .done(function () {
                localStorage.removeItem('token');
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

    function renderProducts(products) {
        const gallery = $('#productGallery');
        gallery.empty();

        products.forEach(product => {
            const card = `
            <div class="col">
                <div class="card h-100 shadow-sm">
                    <img src="${product.imageUrl}" class="card-img-top" alt="${product.productName}" style="height: 200px; object-fit: cover;">
                    <div class="card-body">
                        <h5 class="card-title">${product.productName}</h5>
                        <p class="card-text fw-bold">${product.productPrice} Ft</p>
                    </div>
                </div>
            </div>
        `;
            gallery.append(card);
        });
    }

    form.submit(function (e) {
        e.preventDefault();
        const productName = nameInput.val().trim();
        const productPrice = parseInt(priceInput.val());

        if (productName && !isNaN(productPrice)) {
            $.post('/Product/CreateProduct', { productName: productName, productPrice: productPrice }, function () {
                form[0].reset();
                fetchProducts();
            }).fail(function (error) {
                console.error('Hiba a termék hozzáadásakor:', error);
            });
        }
    });

    $(document).on('click', '.delete-product', function () {
        const id = $(this).data('id');
        if (confirm('Biztosan törlöd ezt a terméket?')) {
            $.post('/Product/DeleteProduct', { productID: id }, function () {
                fetchProducts();
            }).fail(function (error) {
                console.error('Hiba a törlés során:', error);
            });
        }
    });

    fetchProducts();

    const ordersTable = $('#ordersTableBody');
    const orderForm = $('#addOrderForm');
    const customerInput = $('#customerName');
    const productIDInput = $('#productID');
    const priceProduct = $('#price');
    const productAmount = $('#amount');
    function fetchOrders() {
        $.get('/Order/GetAllOrders', function (data) {
            renderOrders(data);
        }).fail(function (error) {
            console.error('Hiba a rendelések lekérésekor:', error);
        });
    }

    orderForm.submit(function (e) {
        e.preventDefault();

        const customerName = customerInput.val().trim();
        const productID = parseInt(productIDInput.val());
        const price = parseInt(priceProduct.val());
        const amount = parseInt(productAmount.val());
        const status = "Függőben";
        const orderDate = new Date().toISOString().split('T')[0];
        const editingId = orderForm.data('editing-id');

        if (customerName && !isNaN(productID) && !isNaN(price)) {
            const orderData = {
                CustomerName: customerName,
                ProductID: productID,
                Price: price,
                Amount: amount,
                Status: status,
                OrderDate: orderDate,
            };

            if (editingId) {
                orderData.OrderID = editingId;
                $.post('/Order/UpdateOrder', orderData, function () {
                    orderForm[0].reset();
                    orderForm.removeData('editing-id');
                    fetchOrders();
                }).fail(function (error) {
                    console.error('Hiba a rendelés frissítésekor:', error);
                });
            } else {
                $.post('/Order/CreateOrder', orderData, function () {
                    orderForm[0].reset();
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
            productIDInput.val(order.productID);
            priceProduct.val(order.price);
            productAmount.val(order.amount);

            orderForm.data('editing-id', id);
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
                <td>${order.productName}</td>
                <td>${order.price} Ft</td>
                <td>${order.amount}</td>
                <td>${order.status}</td>
                <td>${order.orderDate.split('T')[0]}</td>
                <td>
                    <button class="btn btn-warning btn-sm edit-order" data-id="${order.orderID}">
                        ✏️ Szerkesztés
                    </button>
                    <button class="btn btn-danger btn-sm delete-order" data-id="${order.orderID}">
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

    function seedIfEmpty() {
        $.get('/Product/GetAllProducts')
            .done(function (products) {
                if (!products || products.length === 0) {
                    $.post('/Products/SeedProducts')
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
});