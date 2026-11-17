# Defects

Bugs I found in Restful-booker (https://restful-booker.herokuapp.com) on 2026-11-17.
They all came up in the automated tests. DEF-003 and DEF-006 I also checked by hand with curl
to make sure it wasn't a mistake in my tests.

Restful-booker doesn't have a real spec, so "expected" is based on two things: what the
[API docs](https://restful-booker.herokuapp.com/apidoc/index.html) say (for the filter), and normal
HTTP behaviour - bad input should get 400, something that doesn't exist should get 404.

All POST requests send `Content-Type: application/json` and `Accept: application/json`.
The tests for these bugs are marked `[Category("KnownBug")]` and don't run in CI.

---

## DEF-001: Booking with a negative price is accepted

Severity: Medium
Test: `CreateBookingTests.Create_NegativeTotalPrice_Returns400`

**Steps**
1. `POST /booking` with a normal body, but `"totalprice": -1`.
2. Same with `-500`.

**Expected:** 400, no booking created. A price can't be below zero.

**Actual:** 200. The booking is created with the negative price and gets a `bookingid`.

---

## DEF-002: Checkout on the same day as checkin, or before it, is accepted

Severity: Medium
Test: `CreateBookingTests.Create_CheckoutNotAfterCheckin_Returns400`

**Steps**
1. `POST /booking` with `"checkin": "2026-11-27", "checkout": "2026-11-27"`.
2. `POST /booking` with `"checkin": "2026-11-27", "checkout": "2026-11-26"`.

**Expected:** 400 in both cases. A stay should be at least one night, and you can't leave before you arrive.

**Actual:** 200 in both cases, both bookings are created.

---

## DEF-003: Invalid date is saved as "0NaN-aN-aN"

Severity: High
Test: `CreateBookingTests.Create_InvalidDateFormat_Returns400`

**Steps**
1. `POST /booking` with `"checkin": "2026-13-45", "checkout": "2026-12-10"`.
2. Check `booking.bookingdates.checkin` in the response.
3. Search for it: `GET /booking?firstname=<same name>&checkin=2026-12-08`.

**Expected:** 400, the date doesn't exist.

**Actual:** 200, and the booking is stored with `"checkin": "0NaN-aN-aN"`. `"not-a-date"` is accepted
too. On top of that, the broken booking shows up in every `checkin` search, whatever date you ask for,
so it ends up in results where it doesn't belong.

I marked this one High because broken data gets saved and then leaks into other queries.

---

## DEF-004: Empty first name or last name is accepted

Severity: Low
Test: `CreateBookingTests.Create_EmptyName_Returns400`

**Steps**
1. `POST /booking` with `"firstname": ""` and a normal last name.
2. `POST /booking` with a normal first name and `"lastname": ""`.

**Expected:** 400, a booking without a guest name makes no sense.

**Actual:** 200, both bookings are created.

---

## DEF-005: Missing lastname returns 500

Severity: Medium
Test: `CreateBookingTests.Create_WithoutLastName_Returns400`

**Steps**
1. `POST /booking` without the `lastname` field:
   ```json
   {"firstname": "John", "totalprice": 100, "depositpaid": true,
    "bookingdates": {"checkin": "2026-11-22", "checkout": "2026-11-24"}}
   ```

**Expected:** 400, the client sent an incomplete request.

**Actual:** 500 Internal Server Error. The server crashes on input it should just reject.

---

## DEF-006: checkin filter skips bookings on the exact date

Severity: Medium
Test: `GetBookingTests.GetIds_FilterByCheckin_IncludesBookingOnThatDate`

**Steps**
1. `POST /booking` with `"firstname": "CurlCheck7"` and `"checkin": "2026-12-07"`.
2. `GET /booking?firstname=CurlCheck7&checkin=2026-12-06`
3. `GET /booking?firstname=CurlCheck7&checkin=2026-12-07`

**Expected:** the booking is found both times. The docs say the filter returns bookings with checkin
on or after the given date.

**Actual:** found in step 2, missing in step 3. The filter behaves like "after", not "on or after".

---

## DEF-007: Deleting a booking that doesn't exist returns 405

Severity: Low
Test: `UpdateDeleteTests.Delete_NotExistingBooking_Returns404`

**Steps**
1. Get a token: `POST /auth`.
2. `DELETE /booking/999999999` with `Cookie: token=<token>`.

**Expected:** 404 Not Found.

**Actual:** 405 Method Not Allowed. DELETE is allowed, there's just no booking with that id, so the
status code is misleading.
