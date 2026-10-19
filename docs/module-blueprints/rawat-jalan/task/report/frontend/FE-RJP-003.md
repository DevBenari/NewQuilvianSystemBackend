# Laporan Task Frontend — `FE-RJP-003` Lapisan data reservasi

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-003`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-003` |
| Slice | `MVP-1`, `EPIC RP-04` (data) |
| Trace | `FR-RP-030` s.d. `038` (data); `VAL-RP-14`; `RP-FE.7` (link tidak disimpan di state) |
| Kontrak | `RJ-PRI-RESV-001@1.0.0`: `api-contract.md` Outpatient Priority Reservation, Priority Reservation Arrival, hub `/hubs/queues` (`JoinPriorityReservationBoard`, `PriorityReservationChanged`) |
| Dependency | `BE-RJP-006` ✅ |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 19 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji unit 7/7 `PASS`, lint dan build `PASS` |

## 2. Proses bisnis

Task ini tidak punya layar sendiri. Ia menyiapkan jalur data untuk layar Reservasi Prioritas (`FE-RJP-004`, `005`),
mode reservasi di Pendaftaran Pasien (`FE-RJP-006`), dan halaman publik Konfirmasi Kedatangan (`FE-RJP-008`).

Contoh alur: petugas admisi membuka daftar reservasi. Layar memanggil `fetchPriorityReservations` dan
`fetchPriorityReservationSummary`. Saat pasien menekan link di HP, backend mengirim event
`PriorityReservationChanged` ke board. Klien realtime meneruskan event itu ke layar, lalu layar memuat ulang daftar.

Dua aturan dijaga di lapisan ini:

1. **Link konfirmasi tidak disimpan.** Hasil "buat reservasi" dan "kirim ulang link" dikembalikan langsung ke
   pemanggil. Redux hanya menyimpan data reservasi, bukan `arrivalLink`.
2. **409 "baru saja diubah petugas lain" dikenali.** Galat 409 disimpan beserta kode statusnya, sehingga layar
   detail tahu harus memuat ulang data (`VAL-RP-14`).

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/lib/constants/health-services/registration-management/priority-reservation-constants.js` | Baru | Endpoint, status (1–6) beserta label, keadaan halaman publik, nama event, pesan baku, `PRIORITY_RESERVATION_FALLBACK_REFRESH_MS = 60000` |
| `src/lib/services/health-services/registration-management/priority-reservation.service.js` | Baru | 12 fungsi: daftar, ringkasan, detail, kelayakan, buat, kirim ulang link, tandai tiba, kembalikan, skrining, batal, serta `GET`/`POST` publik dengan `_skipAuthRefresh: true` |
| `src/lib/state/slice/health-services/registration-management/priority-reservation-slice.jsx` | Baru | 10 thunk (`fetch…`, `check…Eligibility`, `submit…`), filter, pilihan, galat dengan `status` |
| `src/lib/state/store.jsx` | Diperbarui | Reducer `priorityReservation` didaftarkan |
| `src/lib/realtime/queue-realtime-client.js` | Diperbarui | `PRIORITY_RESERVATION_EVENT_NAME`, `onPriorityReservationEvent`, `joinPriorityReservationBoard`/`leavePriorityReservationBoard`, gabung ulang board sesudah koneksi pulih |
| `src/utils/health-services/registration-management/priority-reservation/priority-reservation.utils.js` | Baru | Pembaca galat (`readPriorityReservationFailure`, `isPriorityReservationStaleConflict`), label/tone status, format tanggal/jam WIB, `normalizeWhatsAppPhone`, `buildWhatsAppUrl`, `maskPhoneNumber`, kartu ringkasan, baris detail, label sumber riwayat |
| `tests/unit/priority-reservation-data-layer.test.mjs` | Baru | 7 uji |

## 4. Gerbang komponen dasar

Task ini tidak menulis JSX atau CSS.

`UI GATE: N/A — lapisan data saja; tidak ada elemen tampilan.`

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (build akhir 19 Oktober 2026, standalone siap) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/priority-reservation-data-layer.test.mjs` | `PASS` 7/7 |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2976/3003; 27 gagal di modul lain (Rawat Inap, Hemodialisa, Bank Darah, Petty Cash, CRM-007). Tiga uji yang membaca `menu-items.jsx` juga gagal pada snapshot `HEAD` tanpa perubahan task ini, jadi bukan regresi |
| `MANUAL TEST` | Tidak ada layar. Lapisan ini dipakai dan terbukti lewat uji layar `FE-RJP-004` s.d. `008` |

### Uji unit

| Uji | Yang dibuktikan |
| --- | --- |
| Kueri | Daftar, ringkasan, detail, dan kelayakan memanggil path dan parameter kontrak |
| Perintah petugas | Tandai tiba, kembalikan, skrining, batal memanggil path kontrak dan menyimpan detail terbaru |
| `VAL-RP-14` | 409 "baru saja diubah" dikenali dan disimpan dengan kode status 409 |
| Link tidak disimpan | Buat dan kirim ulang mengembalikan `arrivalLink` ke pemanggil; state Redux tidak memuatnya (`RP-FE.7`) |
| Halaman publik | `GET`/`POST` arrivals memakai `_skipAuthRefresh: true` |
| Realtime | Klien menyediakan join/leave board dan nama event `PriorityReservationChanged` |
| No. HP | `0812-…`, `+62 812 …`, dan `812…` dinormalkan ke `62812…` untuk `wa.me`. Telepon rumah `021-…` dan isian kosong menghasilkan string kosong. Teks pesan di-encode (`&` menjadi `%26`). Tanpa nomor, tautan menjadi `wa.me/?text=…` sehingga petugas memilih kontak sendiri |

## 6. Delta terhadap kontrak

Tidak ada. Path, method, isi request, dan kode status mengikuti `api-contract.md` `RJ-PRI-RESV-001@1.0.0`.

## 7. Wewenang dan batas

- Tidak ada perubahan backend.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.
