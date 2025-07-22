
    document.querySelector('.password-toggle').addEventListener('click', function() {
                const passwordInput = document.querySelector('#password');
    const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
    passwordInput.setAttribute('type', type);
    this.textContent = type === 'password' ? '👁️' : '🙈';
            });
    document.querySelector('#password').addEventListener('input', function() {
                const strengthMeter = document.querySelector('.strength-meter-fill');
    const strengthText = document.querySelector('.strength-text');
    const value = this.value;
    let strength = 'weak';
                if (value.length >= 12) strength = 'strong';
                else if (value.length >= 8) strength = 'good';
                else if (value.length >= 4) strength = 'fair';
    strengthMeter.className = `strength-meter-fill strength-${strength}`;
    strengthText.innerHTML = `Độ mạnh mật khẩu <small>(${strength === 'weak' ? 'Yếu' : strength === 'fair' ? 'Trung bình' : strength === 'good' ? 'Tốt' : 'Mạnh'})</small>`;
            });
