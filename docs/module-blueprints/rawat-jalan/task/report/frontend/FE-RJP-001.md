# Laporan Task Frontend — `FE-RJP-001` Layar display

Definisi task: [priority-reservation-frontend-roadmap.md](../../../roadmap/priority-reservation-frontend-roadmap.md) bagian `FE-RJP-001`.

## 1. Ringkasan

| Field | Isi |
| --- | --- |
| Task ID | `FE-RJP-001` |
| Slice | `MVP-0`, `EPIC RP-02` |
| Trace | `FR-RP-013`; `RJ-PRI-DEC-005`, `025`; `CONF-RP-01`; `UAT-RP-03` |
| Kontrak | `RJ-PRI-RESV-001@1.0.0` `approved` — `api-contract.md` hub `/hubs/queues` (`JoinQueueDisplayCluster`, `LeaveQueueDisplayCluster`, event `QueueDisplayChanged`) |
| Dependency | `BE-RJP-004` ✅ ([laporan](../backend/BE-RJP-004.md)) |
| Task mode | `FRONTEND` (`CROSS-REPO MODE`, `RJ-PRI-DEC-031`) |
| Branch / baseline | FE `sukmagpV2` @ `17e0ffec0`, working tree belum di-commit |
| Tanggal | 16 Oktober 2026 |
| Model | Claude Opus 5 |
| Status | ✅ Selesai — uji layar 12/12 `PASS`, lint dan build `PASS`. **Rilis wajib bersama `BE-RJP-004`** |

## 2. Masalah yang diperbaiki

Layar display di ruang tunggu sebelumnya bergabung ke group realtime ruang kerja perawat. Akibatnya layar menerima
payload lengkap, termasuk antrean prioritas, lalu menampilkan panggilan langsung dari payload itu. Nomor prioritas bisa
muncul sekilas sebelum data display dimuat ulang (`CONF-RP-01`).

Sesudah perbaikan, layar display bergabung ke group khusus display. Backend (`BE-RJP-004`) hanya mengirim kabar antrean
Reguler ke group itu dengan payload ringkas. Frontend menambah lapis kedua: kabar yang masih membawa tanda antrean
prioritas dibuang sebelum diproses.

Contoh: perawat memanggil nomor 35 (pasien Gold, prioritas) lalu nomor 27 (Reguler). Layar display tetap kosong saat
nomor 35 dipanggil, lalu menampilkan dan menyuarakan nomor 27.

## 3. Perubahan

| Berkas (repo frontend) | Status | Isi |
| --- | --- | --- |
| `src/lib/realtime/queue-realtime-client.js` | Diperbarui | Konstanta `QUEUE_DISPLAY_EVENT_NAME = "QueueDisplayChanged"`; opsi `onQueueDisplayEvent`; `joinQueueDisplayClusters`, `leaveQueueDisplayClusters`, `syncQueueDisplayClusters` (memanggil `JoinQueueDisplayCluster`/`LeaveQueueDisplayCluster`); bergabung ulang ke group display saat koneksi pulih; state group direset saat koneksi dihentikan |
| `src/lib/hooks/queue-display/use-queue-display-screen.jsx` | Diperbarui | Memakai `onQueueDisplayEvent` dan `syncQueueDisplayClusters` sebagai pengganti `onQueueRealtimeEvent` dan `syncNurseStationClusters`; `scheduleRealtimeRefresh` membuang kabar prioritas di baris pertama |
| `src/utils/queue-display/queue-display-call-coordinator-utils.jsx` | Diperbarui | `isPriorityQueueRealtimeEvent` — memeriksa `isPriorityQueue`/`IsPriorityQueue` pada kabar dan pembungkus `queue`/`data`/`payload` |
| `tests/unit/queue-display-priority-filter.test.mjs` | Baru | 4 uji fungsi penyaring |

Yang tidak diubah: tata letak layar display, panel panggilan, daftar antrean, dan suara panggilan. Pemuatan ulang data
display tetap lewat endpoint `queue-display-runtime` yang sudah disaring backend (`BE-RJP-003`).

## 4. Gerbang komponen dasar

Task ini tidak menambah atau mengubah elemen tampilan. Perubahan hanya pada lapisan realtime dan hook.

| Elemen | Keputusan | Bukti |
| --- | --- | --- |
| Layar display (panel panggilan, daftar antrean) | `REUSE` — tidak disentuh | `src/components/view/queue-display/**` tidak ada di diff |

`UI GATE: PASS — tidak ada elemen tampilan baru; tidak ada keputusan yang menunggu user.`

## 5. Validasi

| Langkah | Hasil |
| --- | --- |
| `npm run lint:errors` | exit 0 |
| `npm run build` | exit 0 (Next.js 16.2.12, standalone siap) |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/queue-display-priority-filter.test.mjs` | `PASS` 4/4 |
| `AUTOMATED TEST: node --import ./tests/helpers/register.mjs --test tests/unit/*.test.mjs` | 2969/2996; 27 gagal di modul lain (Rawat Inap, Hemodialisa, Bank Darah, CRM, parity, sidebar Keuangan). Tidak ada yang mengimpor berkas task ini |
| `MANUAL TEST` (Playwright, build standalone port 3000, backend uji 7185) | `PASS` 12/12, rincian di bawah |

`npm run test:unit` tidak dapat dijalankan di Windows karena pola glob-nya tidak diurai shell. Suite dijalankan dengan
perintah setara di atas.

### Uji layar

Data samaran: dua pasien uji baru (`RJPF UJI REG ...` dan `RJPF UJI GOLD ...`), membership Gold disisipkan lewat SQL
data uji, dan satu perangkat display uji `UJI RJPF DISPLAY ...` beserta akun loginnya dibuat lewat API admin. Halaman
`/queue-display` dibuka dengan akun display itu. Panggilan HTTP diarahkan ke backend uji lewat route Playwright. Untuk
WebSocket SignalR, proxy TCP sementara `7184 → 7185` dijalankan selama uji lalu dihentikan.

| ID | Pemeriksaan | Hasil |
| --- | --- | --- |
| F1-1 | Layar bergabung lewat `JoinQueueDisplayCluster` | `PASS` |
| F1-2 | Layar tidak memanggil `JoinNurseStationCluster` | `PASS` |
| F1-3 | Pendaftaran pasien Reguler memicu `QueueDisplayChanged` di layar | `PASS` (1 kabar) |
| F1-4 | Pendaftaran pasien Gold (nomor 35, kelipatan 5) tidak memicu kabar apa pun | `PASS` |
| F1-5 | Layar tidak menerima kabar ruang kerja perawat (`QueueRealtimeEvent`) | `PASS` |
| F1-6 | Tidak ada galat hub saat bergabung | `PASS` |
| F1-7 | Kode antrean prioritas `I035` tidak ada di layar | `PASS` |
| F1-8 | Perawat memanggil antrean prioritas: backend `200` | `PASS` |
| F1-9 | **`UAT-RP-03`**: selama 8 detik sesudah panggilan, isi layar diperiksa setiap 150 ms; `I035` tidak muncul sesaat pun | `PASS` |
| F1-10 | Tidak ada kabar display maupun permintaan audio untuk panggilan prioritas | `PASS` (audio 0) |
| F1-11 | **`UAT-RP-03`**: perawat memanggil antrean Reguler `I027`; nomor tampil di panel *Antrean Perawat* | `PASS` |
| F1-12 | Panggilan Reguler memicu pengambilan audio suara panggilan dari backend | `PASS` |

Tangkapan layar: `fe-rjp-001-called.png` di scratchpad sesi (panel *Antrean Perawat* menampilkan `I027`; daftar tunggu
tidak memuat nomor kelipatan 5).

## 6. Wewenang dan batas

- Wewenang UI: tidak ada keputusan tampilan baru. Bunyi dan tata letak tetap seperti sebelumnya.
- Suara diperiksa sampai permintaan audio ke backend. Bunyi keluar dari pengeras suara tidak dapat diamati di browser
  tanpa layar; perlu dicek sekali di perangkat display nyata saat UAT.
- Suntingan user yang belum di-commit di `emergency-registration/patient-selection-step.jsx` tidak disentuh.
- Tidak ada stage, commit, push, merge, atau deploy.

## 7. Risiko dan rilis

| Risiko | Penanganan |
| --- | --- |
| Frontend dirilis tanpa `BE-RJP-004` | Hub lama tidak mengenal `JoinQueueDisplayCluster`, sehingga layar tidak menerima kabar realtime sama sekali. Layar masih memuat ulang data berkala. **Rilis berdua** |
| `BE-RJP-004` dirilis tanpa task ini | Akun display ditolak di group perawat, sehingga layar lama kehilangan kabar realtime. **Rilis berdua** |
| Payload lama masih membawa antrean prioritas | Lapis kedua `isPriorityQueueRealtimeEvent` membuangnya |
